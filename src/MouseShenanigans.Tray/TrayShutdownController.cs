using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayShutdownController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly IDisposable? localControlHost;
    private readonly Action hideTrayIcon;
    private readonly Action disposeExitResources;
    private readonly Action exitThread;
    private readonly Action? forceExit;
    private readonly TimeSpan forceExitDelay;
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
        IDisposable? localControlHost = null,
        Action? forceExit = null,
        TimeSpan? forceExitDelay = null)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.localControlHost = localControlHost;
        this.hideTrayIcon = hideTrayIcon ?? throw new ArgumentNullException(nameof(hideTrayIcon));
        this.disposeExitResources = disposeExitResources ?? throw new ArgumentNullException(nameof(disposeExitResources));
        this.exitThread = exitThread ?? throw new ArgumentNullException(nameof(exitThread));
        this.forceExit = forceExit;
        this.forceExitDelay = forceExitDelay ?? Timeout.InfiniteTimeSpan;
    }

    public void RequestExit()
    {
        if (exitRequested)
        {
            return;
        }

        exitRequested = true;
        ScheduleForcedExit();
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
                    try
                    {
                        hideTrayIcon();
                    }
                    finally
                    {
                        exitThread();
                    }
                }
            }
        }
    }

    private void ScheduleForcedExit()
    {
        if (forceExit is null || forceExitDelay == Timeout.InfiniteTimeSpan)
        {
            return;
        }

        _ = ForceExitAfterDelayAsync();
    }

    private async Task ForceExitAfterDelayAsync()
    {
        await Task.Delay(forceExitDelay).ConfigureAwait(false);
        forceExit?.Invoke();
    }
}
