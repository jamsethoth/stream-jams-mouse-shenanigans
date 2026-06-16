using System.Diagnostics;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class CommandRunner
{
    public static CommandResult Run(string fileName, IEnumerable<string> arguments, TimeSpan timeout)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = RepositoryPaths.Root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        process.StartInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        foreach (string argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start '{fileName}'.");
        }

        var outputBuffer = new ProcessOutputBuffer();
        outputBuffer.Attach(process);
        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            throw new TimeoutException(
                $"Command '{fileName} {string.Join(" ", arguments)}' timed out after {timeout}.");
        }

        process.WaitForExit();
        return new CommandResult(process.ExitCode, outputBuffer.StandardOutput, outputBuffer.StandardError);
    }
}

internal sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
{
    public void EnsureSuccess(string description)
    {
        if (ExitCode == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{description} failed with exit code {ExitCode}.\nSTDOUT:\n{StandardOutput}\nSTDERR:\n{StandardError}");
    }
}
