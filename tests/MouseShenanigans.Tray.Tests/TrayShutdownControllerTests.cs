using MouseShenanigans.Tray;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayShutdownControllerTests
{
    [Fact]
    public void RequestExitHidesTrayIconDisposesRuntimeAndExitsThread()
    {
        var runtime = new RecordingRuntimeController();
        var trayIconHidden = false;
        var exitRequests = 0;
        var controller = new TrayShutdownController(
            runtime,
            hideTrayIcon: () => trayIconHidden = true,
            exitThread: () => exitRequests++);

        controller.RequestExit();

        Assert.True(trayIconHidden);
        Assert.True(runtime.IsDisposed);
        Assert.Equal(1, exitRequests);
    }

    [Fact]
    public void RequestExitDisposesExitResourcesBeforeRuntimeAndExitThread()
    {
        var events = new List<string>();
        var runtime = new RecordingRuntimeController(() => events.Add("runtime"));
        var controller = new TrayShutdownController(
            runtime,
            hideTrayIcon: () => events.Add("hide"),
            disposeExitResources: () => events.Add("exit-resources"),
            exitThread: () => events.Add("thread"));

        controller.RequestExit();

        Assert.Equal(["exit-resources", "runtime", "hide", "thread"], events);
    }

    [Fact]
    public void RequestExitIsIdempotent()
    {
        var runtime = new RecordingRuntimeController();
        var hideRequests = 0;
        var exitRequests = 0;
        var controller = new TrayShutdownController(
            runtime,
            hideTrayIcon: () => hideRequests++,
            exitThread: () => exitRequests++);

        controller.RequestExit();
        controller.RequestExit();

        Assert.Equal(1, hideRequests);
        Assert.Equal(1, runtime.DisposeRequests);
        Assert.Equal(1, exitRequests);
    }

    [Fact]
    public void RequestExitDisposesLocalControlBeforeRuntime()
    {
        List<string> operations = [];
        var runtime = new RecordingRuntimeController(() => operations.Add("runtime-dispose"));
        var localControl = new RecordingLocalControlHost(operations);
        var controller = new TrayShutdownController(
            runtime,
            hideTrayIcon: () => operations.Add("hide"),
            exitThread: () => operations.Add("exit"),
            localControlHost: localControl);

        controller.RequestExit();

        Assert.Equal(["local-control-dispose", "runtime-dispose", "hide", "exit"], operations);
    }

    [Fact]
    public void RequestExitRunsForcedExitWhenCleanupDoesNotFinishBeforeDelay()
    {
        using var cleanupStarted = new ManualResetEventSlim();
        using var forceExitRan = new ManualResetEventSlim();
        var runtime = new RecordingRuntimeController(() =>
        {
            cleanupStarted.Set();
            forceExitRan.Wait(TimeSpan.FromSeconds(2));
        });
        var controller = new TrayShutdownController(
            runtime,
            hideTrayIcon: () => { },
            disposeExitResources: () => { },
            exitThread: () => { },
            forceExit: forceExitRan.Set,
            forceExitDelay: TimeSpan.FromMilliseconds(25));
        var thread = new Thread(controller.RequestExit);

        thread.Start();

        Assert.True(cleanupStarted.Wait(TimeSpan.FromSeconds(2)));
        Assert.True(forceExitRan.Wait(TimeSpan.FromSeconds(2)));
        Assert.True(thread.Join(TimeSpan.FromSeconds(2)));
    }

    private sealed class RecordingRuntimeController(Action? disposeAction = null) : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; } = RuntimeRemappingStatus.Disabled;

        public bool IsCursorLockEnabled { get; private set; }

        public int DisposeRequests { get; private set; }

        public bool IsDisposed => DisposeRequests > 0;

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void ApplyOptions(RuntimeRemappingOptions options)
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Dispose()
        {
            DisposeRequests++;
            disposeAction?.Invoke();
        }
    }

    private sealed class RecordingLocalControlHost(List<string> operations) : IDisposable
    {
        public void Dispose()
        {
            operations.Add("local-control-dispose");
        }
    }
}
