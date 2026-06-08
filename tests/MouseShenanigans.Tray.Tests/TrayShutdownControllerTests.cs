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

    private sealed class RecordingRuntimeController : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; } = RuntimeRemappingStatus.Disabled;

        public int DisposeRequests { get; private set; }

        public bool IsDisposed => DisposeRequests > 0;

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Dispose()
        {
            DisposeRequests++;
        }
    }
}
