using MouseShenanigans.Tray;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayCursorLockControllerTests
{
    [Fact]
    public void SetCursorLockEnabledUpdatesRuntimeAndRefreshesStatus()
    {
        var runtime = new RecordingRuntimeController();
        var refreshRequests = 0;
        var controller = new TrayCursorLockController(runtime, () => refreshRequests++);

        controller.SetCursorLockEnabled(true);

        Assert.True(runtime.IsCursorLockEnabled);
        Assert.Equal(1, refreshRequests);
    }

    private sealed class RecordingRuntimeController : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; } = RuntimeRemappingStatus.Disabled;

        public bool IsCursorLockEnabled { get; private set; }

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Dispose()
        {
        }
    }
}
