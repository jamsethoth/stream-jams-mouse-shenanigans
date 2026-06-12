using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayShutdownController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly IDisposable? localControlHost;
    private readonly Action hideTrayIcon;
    private readonly Action disposeExitResources;
    private readonly Action exitThread;
    private bool exitRequested;

    public TrayShutdownController(
        IRuntimeRemappingController runtime,
        Action hideTrayIcon,
        Action exitThread)
        : this(runtime, hideTrayIcon, disposeExitResources: static () => { }, exitThread, localControlHost: null)
    {
    }

    public TrayShutdownController(
        IRuntimeRemappingController runtime,
        Action hideTrayIcon,
        Action exitThread,
        IDisposable? localControlHost = null)
        : this(runtime, hideTrayIcon, disposeExitResources: static () => { }, exitThread, localControlHost)
    {
    }

    public TrayShutdownController(
        IRuntimeRemappingController runtime,
        Action hideTrayIcon,
        Action disposeExitResources,
        Action exitThread,
        IDisposable? localControlHost = null)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.localControlHost = localControlHost;
        this.hideTrayIcon = hideTrayIcon ?? throw new ArgumentNullException(nameof(hideTrayIcon));
        this.disposeExitResources = disposeExitResources ?? throw new ArgumentNullException(nameof(disposeExitResources));
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
        try
        {
            disposeExitResources();
        }
        finally
        {
            try
            {
                localControlHost?.Dispose();
            }
            finally
            {
                try
                {
                    runtime.Dispose();
                }
                finally
                {
                    exitThread();
                }
            }
        }
    }
}
