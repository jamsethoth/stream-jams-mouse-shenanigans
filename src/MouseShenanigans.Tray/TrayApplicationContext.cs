using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly TrayStartupOptions startupOptions;
    private readonly IDiagnosticRecorder diagnosticRecorder;
    private readonly AbsoluteCursorRemappingCoordinator runtime;
    private readonly RuntimeCommandController runtimeCommandController;
    private readonly ForegroundAllowlistConfirmationController foregroundAllowlistConfirmationController;
    private readonly TrayForegroundAllowlistConfirmationPresenter foregroundAllowlistConfirmationPresenter;
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
    private readonly ApplicationSafetySentinel safetySentinel;
    private readonly System.Windows.Forms.Timer safetySentinelTimer;
    private readonly NotifyIcon notifyIcon;
    private bool disposed;

    public TrayApplicationContext()
        : this(TrayStartupOptions.FromEnvironment())
    {
    }

    public TrayApplicationContext(TrayStartupOptions startupOptions)
    {
        this.startupOptions = startupOptions ?? throw new ArgumentNullException(nameof(startupOptions));
        diagnosticRecorder = CreateDiagnosticRecorder(this.startupOptions);
        RecordStartupValidationMessages(this.startupOptions, diagnosticRecorder);
        runtimeConfigurationController = CreateRuntimeConfigurationController(this.startupOptions, diagnosticRecorder);
        TargetWindowReader targetWindowReader = new();
        foregroundAllowlistConfirmationController = new ForegroundAllowlistConfirmationController(
            runtimeConfigurationController,
            targetWindowReader);
        runtime = CreateRuntime(runtimeConfigurationController.Current.CreateRuntimeOptions());
        runtimeCommandController = new RuntimeCommandController(
            runtime,
            runtimeConfigurationController,
            targetWindowReader,
            enableApplicationSafety: true,
            foregroundAllowlistConfirmationController: foregroundAllowlistConfirmationController,
            diagnosticRecorder: diagnosticRecorder);
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
            openFolder: null,
            UpdateRuntimeStatus);
        foregroundAllowlistConfirmationPresenter = new TrayForegroundAllowlistConfirmationPresenter(
            foregroundAllowlistConfirmationController,
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
            UpdateRuntimeStatus,
            foregroundAllowlistConfirmationPresenter.ShowConfirmation);
        hotkeyReceiver = new TrayHotkeyReceiver(hotkeyController.DispatchHotkey);
        localControlHost = CreateLocalControlHost(
            runtimeCommandController,
            UpdateRuntimeStatus,
            this.startupOptions,
            diagnosticRecorder,
            foregroundAllowlistConfirmationPresenter.ShowConfirmation);
        shutdownController = new TrayShutdownController(
            runtime,
            HideNotifyIcon,
            DisposeExitResources,
            ExitThread,
            localControlHost,
            forceExit: static () => Environment.Exit(0),
            forceExitDelay: TimeSpan.FromSeconds(5));
        safetySentinel = new ApplicationSafetySentinel(
            () => runtimeConfigurationController.Current,
            new ProcessSnapshotReader(),
            runtimeCommandController.EmergencyDisable,
            () => shutdownController.RequestExit(),
            isRuntimeEnabled: () => runtime.Status.State == RuntimeRemappingState.Enabled,
            diagnosticRecorder: diagnosticRecorder);
        safetySentinelTimer = new System.Windows.Forms.Timer
        {
            Interval = (int)this.startupOptions.SelfExitSentinelInterval.TotalMilliseconds,
        };
        safetySentinelTimer.Tick += (_, _) =>
        {
            safetySentinel.EvaluateOnce();
            UpdateRuntimeStatus();
        };

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = CreateContextMenu(),
            Icon = SystemIcons.Application,
            Visible = true,
        };

        localControlHost.Start();
        safetySentinelTimer.Start();
        hotkeyController.Register(
            hotkeyReceiver.WindowHandle,
            DefaultRuntimeHotkeyBindings.All);
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
            safetySentinelTimer.Dispose();
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
            localControlHost.Status.Message,
            startupOptions.ValidationMessage,
            runtimeCommandController.ApplicationSafetyStatusMessage,
            safetySentinel.StatusMessage);
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
        safetySentinelTimer.Stop();
        safetySentinelTimer.Dispose();
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
            TimeProvider.System,
            WindowsRuntime.IsDesktopInputAvailable);
    }

    private static RuntimeConfigurationController CreateRuntimeConfigurationController(
        TrayStartupOptions startupOptions,
        IDiagnosticRecorder diagnosticRecorder)
    {
        IRuntimeConfigurationStore store = startupOptions.HasInvalidRuntimeConfigurationPathOverride
            ? new InvalidRuntimeConfigurationStore(
                startupOptions.RuntimeConfigurationPath ?? "<invalid override>",
                startupOptions.RuntimeConfigurationPathError!)
            : new RuntimeConfigurationFileStore(
                RuntimeConfigurationFileStore.CreateDefaultConfigurationPath(startupOptions.RuntimeConfigurationPath));

        return new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration(),
            diagnosticRecorder);
    }

    private static LocalControlHost CreateLocalControlHost(
        RuntimeCommandController commandController,
        Action refreshStatus,
        TrayStartupOptions startupOptions,
        IDiagnosticRecorder diagnosticRecorder,
        Action<ForegroundAllowlistConfirmationRequest> requestForegroundAllowlistConfirmationPrompt)
    {
        SynchronizationContext? synchronizationContext = SynchronizationContext.Current;
        var handler = new LocalControlEndpointHandler(
            commandController,
            diagnosticRecorder,
            getDegradedStatusMessage: () => startupOptions.ValidationMessage,
            requestStatusRefresh: () => RunOnSynchronizationContext(synchronizationContext, refreshStatus),
            runRequestOnControlThread: operation => RunOnSynchronizationContext(synchronizationContext, operation),
            requestForegroundAllowlistConfirmationPrompt: request =>
                PostOnSynchronizationContext(
                    synchronizationContext,
                    () => requestForegroundAllowlistConfirmationPrompt(request)));

        return new LocalControlHost(
            startupOptions.LocalControlOptions ?? LocalControlOptions.Default,
            handler,
            new KestrelLocalControlWebApplicationFactory(),
            diagnosticRecorder,
            startupOptions.LocalControlUrlError);
    }

    private static BoundedDiagnosticRecorder CreateDiagnosticRecorder(TrayStartupOptions startupOptions)
    {
        return new BoundedDiagnosticRecorder(jsonLinesPath: startupOptions.DiagnosticsPath);
    }

    private static void RecordStartupValidationMessages(
        TrayStartupOptions startupOptions,
        IDiagnosticRecorder diagnosticRecorder)
    {
        foreach (string message in startupOptions.ValidationMessages)
        {
            diagnosticRecorder.Record(DiagnosticEventTypes.StartupOverrideInvalid, message);
        }
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

    private static void PostOnSynchronizationContext(SynchronizationContext? synchronizationContext, Action action)
    {
        if (synchronizationContext is null)
        {
            _ = Task.Run(action);
            return;
        }

        synchronizationContext.Post(_ => action(), null);
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
