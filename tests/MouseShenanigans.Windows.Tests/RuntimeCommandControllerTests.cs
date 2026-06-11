using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeCommandControllerTests
{
    [Fact]
    public void ToggleEnablesDisabledRuntime()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime);

        controller.Toggle();

        Assert.Equal(1, runtime.EnableRequests);
        Assert.Equal(0, runtime.DisableRequests);
    }

    [Fact]
    public void ToggleDisablesEnabledRuntime()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var controller = new RuntimeCommandController(runtime);

        controller.Toggle();

        Assert.Equal(0, runtime.EnableRequests);
        Assert.Equal(1, runtime.DisableRequests);
    }

    [Fact]
    public void EmergencyDisableUsesRuntimeDisablePathWhenEnabled()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var controller = new RuntimeCommandController(runtime);

        controller.EmergencyDisable();

        Assert.Equal(1, runtime.DisableRequests);
    }

    [Fact]
    public void EmergencyDisableUsesRuntimeDisablePathWhenAlreadyDisabled()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime);

        controller.EmergencyDisable();

        Assert.Equal(1, runtime.DisableRequests);
    }

    private sealed class RecordingRuntimeController(RuntimeRemappingStatus status) : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; private set; } = status;

        public bool IsCursorLockEnabled { get; private set; }

        public int EnableRequests { get; private set; }

        public int DisableRequests { get; private set; }

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void Enable()
        {
            EnableRequests++;
            Status = RuntimeRemappingStatus.Enabled;
        }

        public void Disable()
        {
            DisableRequests++;
            Status = RuntimeRemappingStatus.Disabled;
        }

        public void Dispose()
        {
        }
    }
}
