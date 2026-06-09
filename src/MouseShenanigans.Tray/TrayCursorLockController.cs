using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayCursorLockController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly Action refreshStatus;

    public TrayCursorLockController(IRuntimeRemappingController runtime, Action refreshStatus)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.refreshStatus = refreshStatus ?? throw new ArgumentNullException(nameof(refreshStatus));
    }

    public void SetCursorLockEnabled(bool enabled)
    {
        runtime.SetCursorLockEnabled(enabled);
        refreshStatus();
    }
}
