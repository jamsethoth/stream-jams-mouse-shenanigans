using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayShutdownController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly Action hideTrayIcon;
    private readonly Action exitThread;
    private bool exitRequested;

    public TrayShutdownController(
        IRuntimeRemappingController runtime,
        Action hideTrayIcon,
        Action exitThread)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.hideTrayIcon = hideTrayIcon ?? throw new ArgumentNullException(nameof(hideTrayIcon));
        this.exitThread = exitThread ?? throw new ArgumentNullException(nameof(exitThread));
    }

    public void RequestExit()
    {
        if (exitRequested)
        {
            return;
        }

        exitRequested = true;
        hideTrayIcon();
        runtime.Dispose();
        exitThread();
    }
}
