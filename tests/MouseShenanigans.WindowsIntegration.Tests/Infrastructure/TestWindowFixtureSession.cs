using System.Diagnostics;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class TestWindowFixtureSession : IAsyncDisposable
{
    private readonly Process process;
    private readonly TemporaryDirectory rootDirectory;
    private bool disposed;

    private TestWindowFixtureSession(Process process, TemporaryDirectory rootDirectory, string windowTitle)
    {
        this.process = process;
        this.rootDirectory = rootDirectory;
        WindowTitle = windowTitle;
    }

    public string WindowTitle { get; }

    public static async Task<TestWindowFixtureSession> StartAsync(
        PublishedApplication application,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        TemporaryDirectory rootDirectory = TemporaryDirectory.Create("fixture-session");
        string windowTitle = $"Mouse Shenanigans Integration Fixture {Guid.NewGuid():N}";
        string readyFilePath = Path.Combine(rootDirectory.DirectoryPath, "fixture.ready");

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = application.ExecutablePath,
            WorkingDirectory = application.PublishDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false,
        };
        process.StartInfo.ArgumentList.Add("--title");
        process.StartInfo.ArgumentList.Add(windowTitle);
        process.StartInfo.ArgumentList.Add("--ready-file");
        process.StartInfo.ArgumentList.Add(readyFilePath);

        if (!process.Start())
        {
            rootDirectory.Dispose();
            throw new InvalidOperationException($"Failed to start '{application.ExecutablePath}'.");
        }

        await WaitForReadyFileAsync(readyFilePath, windowTitle, cancellationToken);
        return new TestWindowFixtureSession(process, rootDirectory, windowTitle);
    }

    public Task FocusAsync(CancellationToken cancellationToken = default)
    {
        return DesktopWindowController.FocusWindowAsync(WindowTitle, TimeSpan.FromSeconds(5), cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

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

    private static async Task WaitForReadyFileAsync(
        string readyFilePath,
        string expectedTitle,
        CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(readyFilePath)
                && File.ReadAllText(readyFilePath).Contains(expectedTitle, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new TimeoutException($"Test window fixture did not become ready at '{readyFilePath}'.");
    }
}
