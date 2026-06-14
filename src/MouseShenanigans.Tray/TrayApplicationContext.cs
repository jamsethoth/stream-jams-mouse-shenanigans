using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AbsoluteCursorRemappingCoordinator runtime;
    private readonly RuntimeCommandController runtimeCommandController;
    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem hotkeyStatusItem;
    private readonly ToolStripMenuItem enableItem;
    private readonly ToolStripMenuItem disableItem;
    private readonly ToolStripMenuItem cursorLockItem;
    private readonly ToolStripMenuItem profileMenuItem;
    private readonly ToolStripMenuItem reloadConfigurationItem;
    private readonly ToolStripMenuItem openConfigurationFolderItem;
    private readonly RuntimeConfigurationController runtimeConfigurationController;
    private readonly TrayCursorLockController cursorLockController;
    private readonly TrayProfileMenuController profileMenuController;
    private readonly TrayConfigurationFolderController configurationFolderController;
    private readonly TrayHotkeyController hotkeyController;
    private readonly TrayHotkeyReceiver hotkeyReceiver;
    private readonly LocalControlHost localControlHost;
    private readonly TrayShutdownController shutdownController;
    private readonly NotifyIcon notifyIcon;
    private bool disposed;

    public TrayApplicationContext()
    {
        runtimeConfigurationController = CreateRuntimeConfigurationController();
        runtime = CreateRuntime(runtimeConfigurationController.Current.CreateRuntimeOptions());
        runtimeCommandController = new RuntimeCommandController(
            runtime,
            runtimeConfigurationController,
            new TargetWindowReader());
        statusItem = new ToolStripMenuItem { Enabled = false };
        hotkeyStatusItem = new ToolStripMenuItem { Enabled = false };
        enableItem = new ToolStripMenuItem("Enable remapping");
        disableItem = new ToolStripMenuItem("Disable remapping");
        profileMenuItem = new ToolStripMenuItem("Profiles");
        reloadConfigurationItem = new ToolStripMenuItem("Reload configuration");
        openConfigurationFolderItem = new ToolStripMenuItem("Open configuration folder");
        cursorLockItem = new ToolStripMenuItem("Lock cursor to target")
        {
            CheckOnClick = true,
        };
        cursorLockController = new TrayCursorLockController(runtime, UpdateRuntimeStatus);
        profileMenuController = new TrayProfileMenuController(
            profileMenuItem,
            runtimeCommandController,
            UpdateRuntimeStatus);
        configurationFolderController = new TrayConfigurationFolderController(
            runtimeConfigurationController,
            new ExplorerConfigurationFolderLauncher(),
            UpdateRuntimeStatus);

        enableItem.Click += (_, _) =>
        {
            runtimeCommandController.Enable();
            UpdateRuntimeStatus();
        };
        disableItem.Click += (_, _) =>
        {
            runtimeCommandController.Disable();
            UpdateRuntimeStatus();
        };
        cursorLockItem.Click += (_, _) => cursorLockController.SetCursorLockEnabled(cursorLockItem.Checked);
        reloadConfigurationItem.Click += (_, _) => profileMenuController.ReloadConfiguration();
        openConfigurationFolderItem.Click += (_, _) => configurationFolderController.OpenConfigurationFolder();
        hotkeyController = new TrayHotkeyController(
            new WindowsHotkeyRegistrar(),
            runtimeCommandController,
            UpdateRuntimeStatus);
        hotkeyReceiver = new TrayHotkeyReceiver(hotkeyController.DispatchHotkey);
        localControlHost = CreateLocalControlHost(runtimeCommandController, UpdateRuntimeStatus);
        shutdownController = new TrayShutdownController(
            runtime,
            HideNotifyIcon,
            DisposeExitResources,
            ExitThread,
            localControlHost,
            forceExit: static () => Environment.Exit(0),
            forceExitDelay: TimeSpan.FromSeconds(5));

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = CreateContextMenu(),
            Icon = SystemIcons.Application,
            Visible = true,
        };

        localControlHost.Start();
        hotkeyController.Register(
            hotkeyReceiver.WindowHandle,
            DefaultRuntimeHotkeyBindingProvider.Instance.GetBindings());
        UpdateRuntimeStatus();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            localControlHost.Dispose();
            hotkeyController.Dispose();
            hotkeyReceiver.Dispose();
            runtime.Dispose();
            HideNotifyIcon();
            notifyIcon.Dispose();
        }

        disposed = true;
        base.Dispose(disposing);
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => shutdownController.RequestExit();

        return new ContextMenuStrip
        {
            Items =
            {
                statusItem,
                hotkeyStatusItem,
                new ToolStripSeparator(),
                enableItem,
                disableItem,
                cursorLockItem,
                profileMenuItem,
                reloadConfigurationItem,
                openConfigurationFolderItem,
                new ToolStripSeparator(),
                exitItem,
            },
        };
    }

    private void UpdateRuntimeStatus()
    {
        RuntimeRemappingStatus status = runtime.Status;
        notifyIcon.Text = TrayStatusFormatter.CreateTrayText(status);
        statusItem.Text = TrayStatusFormatter.CreateRuntimeStatusText(
            status,
            runtimeConfigurationController.Current,
            runtimeConfigurationController.StatusMessage,
            localControlHost.Status.Message);
        hotkeyStatusItem.Text = TrayStatusFormatter.CreateHotkeyStatusText(
            hotkeyController.RegistrationResult,
            hotkeyController.LastDispatchedCommand,
            hotkeyController.LastReceivedHotkeyId);
        profileMenuController.RefreshProfiles();
        enableItem.Enabled = status.State is RuntimeRemappingState.Disabled or RuntimeRemappingState.Failed;
        disableItem.Enabled = status.State == RuntimeRemappingState.Enabled;
        cursorLockItem.Checked = runtime.IsCursorLockEnabled;
        cursorLockItem.Enabled = status.State != RuntimeRemappingState.Unsupported;
        reloadConfigurationItem.Enabled = status.State != RuntimeRemappingState.Unsupported;
    }

    private void HideNotifyIcon()
    {
        notifyIcon.Visible = false;
    }

    private void DisposeExitResources()
    {
        hotkeyController.Dispose();
        hotkeyReceiver.Dispose();
    }

    private static AbsoluteCursorRemappingCoordinator CreateRuntime(RuntimeRemappingOptions options)
    {
        return new AbsoluteCursorRemappingCoordinator(
            options,
            new RawInputMouseMovementSource(),
            new TargetWindowReader(),
            new WindowsCursorPositionController(),
            new WindowsCursorLockController(),
            SystemRuntimeClock.Instance,
            WindowsRuntime.IsDesktopInputAvailable);
    }

    private static RuntimeConfigurationController CreateRuntimeConfigurationController()
    {
        return new RuntimeConfigurationController(
            new RuntimeConfigurationFileStore(new RuntimeConfigurationPathProvider()),
            RuntimeProofOfConceptDefaults.CreateConfiguration());
    }

    private static LocalControlHost CreateLocalControlHost(
        RuntimeCommandController commandController,
        Action refreshStatus)
    {
        SynchronizationContext? synchronizationContext = SynchronizationContext.Current;
        var handler = new LocalControlEndpointHandler(
            commandController,
            getDegradedStatusMessage: () => null,
            requestStatusRefresh: () => RunOnSynchronizationContext(synchronizationContext, refreshStatus),
            runRequestOnControlThread: operation => RunOnSynchronizationContext(synchronizationContext, operation));

        return new LocalControlHost(
            LocalControlOptions.Default,
            handler,
            new KestrelLocalControlWebApplicationFactory());
    }

    private static void RunOnSynchronizationContext(SynchronizationContext? synchronizationContext, Action action)
    {
        RunOnSynchronizationContext(
            synchronizationContext,
            () =>
            {
                action();
                return true;
            });
    }

    private static T RunOnSynchronizationContext<T>(
        SynchronizationContext? synchronizationContext,
        Func<T> operation)
    {
        if (synchronizationContext is null || SynchronizationContext.Current == synchronizationContext)
        {
            return operation();
        }

        var completion = new TaskCompletionSource<T>();
        synchronizationContext.Post(
            _ =>
            {
                try
                {
                    completion.SetResult(operation());
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            },
            null);

        return completion.Task.GetAwaiter().GetResult();
    }
}
