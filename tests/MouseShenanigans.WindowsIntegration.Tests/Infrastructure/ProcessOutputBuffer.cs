using System.Diagnostics;
using System.Text;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class ProcessOutputBuffer
{
    private readonly StringBuilder standardOutput = new();
    private readonly StringBuilder standardError = new();

    public string StandardOutput
    {
        get
        {
            lock (standardOutput)
            {
                return standardOutput.ToString();
            }
        }
    }

    public string StandardError
    {
        get
        {
            lock (standardError)
            {
                return standardError.ToString();
            }
        }
    }

    public void Attach(Process process)
    {
        process.OutputDataReceived += (_, args) => AppendLine(standardOutput, args.Data);
        process.ErrorDataReceived += (_, args) => AppendLine(standardError, args.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    private static void AppendLine(StringBuilder builder, string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (builder)
        {
            builder.AppendLine(line);
        }
    }
}
