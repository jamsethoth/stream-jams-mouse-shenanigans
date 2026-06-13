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

        Assert.Equal(["hide", "exit-resources", "runtime", "thread"], events);
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
}
