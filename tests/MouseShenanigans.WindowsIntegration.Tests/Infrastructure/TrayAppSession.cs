using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class TrayAppSession : IAsyncDisposable
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly Process process;
    private readonly ProcessOutputBuffer outputBuffer;
    private readonly TemporaryDirectory rootDirectory;
    private bool disposed;

    private TrayAppSession(
        Process process,
        ProcessOutputBuffer outputBuffer,
        TemporaryDirectory rootDirectory,
        TrayAppLaunchOptions launchOptions)
    {
        this.process = process;
        this.outputBuffer = outputBuffer;
        this.rootDirectory = rootDirectory;
        LaunchOptions = launchOptions;
        Client = new LocalControlClient(launchOptions.LocalControlBaseUri);
    }

    public TrayAppLaunchOptions LaunchOptions { get; }

    public LocalControlClient Client { get; }

    public bool HasExited => process.HasExited;

    public static TrayAppSession Start(PublishedApplication application, string? configurationJson = null)
    {
        ArgumentNullException.ThrowIfNull(application);

        TemporaryDirectory rootDirectory = TemporaryDirectory.Create("tray-session");
        using ReservedLoopbackPort port = ReservedLoopbackPort.Reserve();
        TrayAppLaunchOptions launchOptions = TrayAppLaunchOptions.Create(rootDirectory.DirectoryPath, port.BaseUri);
        if (configurationJson is not null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(launchOptions.ConfigurationPath)!);
            File.WriteAllText(launchOptions.ConfigurationPath, configurationJson, Utf8NoBom);
        }

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = application.ExecutablePath,
            WorkingDirectory = application.PublishDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        launchOptions.ApplyEnvironment(process.StartInfo);

        port.Dispose();
        if (!process.Start())
        {
            rootDirectory.Dispose();
            throw new InvalidOperationException($"Failed to start '{application.ExecutablePath}'.");
        }

        var outputBuffer = new ProcessOutputBuffer();
        outputBuffer.Attach(process);
        return new TrayAppSession(process, outputBuffer, rootDirectory, launchOptions);
    }

    public async Task WaitForExitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutCancellation = new CancellationTokenSource(timeout);
        using CancellationTokenSource linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);

        try
        {
            await process.WaitForExitAsync(linkedCancellation.Token);
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"Tray process did not exit within {timeout}.");
        }
    }

    public async Task<string> CreateFailureContextAsync()
    {
        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Tray process id: {process.Id}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Tray has exited: {process.HasExited}");
        if (process.HasExited)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Tray exit code: {process.ExitCode}");
        }

        builder.AppendLine(CultureInfo.InvariantCulture, $"Local control URL: {LaunchOptions.LocalControlBaseUri}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Config path: {LaunchOptions.ConfigurationPath}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"Diagnostics path: {LaunchOptions.DiagnosticsPath}");
        builder.AppendLine("STDOUT:");
        builder.AppendLine(outputBuffer.StandardOutput);
        builder.AppendLine("STDERR:");
        builder.AppendLine(outputBuffer.StandardError);
        builder.AppendLine("Config file:");
        builder.AppendLine(ReadFileIfExists(LaunchOptions.ConfigurationPath));
        builder.AppendLine("Diagnostics file:");
        builder.AppendLine(ReadFileIfExists(LaunchOptions.DiagnosticsPath));

        try
        {
            using JsonDocument diagnostics = await Client.GetDiagnosticsAsync();
            builder.AppendLine("Diagnostics endpoint:");
            builder.AppendLine(diagnostics.RootElement.GetRawText());
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            builder.AppendLine("Diagnostics endpoint unavailable:");
            builder.AppendLine(exception.Message);
        }

        return builder.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        Client.Dispose();
        if (!process.HasExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
            catch (InvalidOperationException)
            {
            }
        }

        process.Dispose();
        rootDirectory.Dispose();
        disposed = true;
    }

    private static string ReadFileIfExists(string path)
    {
        try
        {
            return File.Exists(path)
                ? File.ReadAllText(path, Encoding.UTF8)
                : "<missing>";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return $"<unavailable: {exception.Message}>";
        }
    }
}
