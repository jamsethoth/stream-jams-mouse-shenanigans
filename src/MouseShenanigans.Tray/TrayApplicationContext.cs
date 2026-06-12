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
    private readonly TrayCursorLockController cursorLockController;
    private readonly TrayHotkeyController hotkeyController;
    private readonly TrayHotkeyReceiver hotkeyReceiver;
    private readonly TrayShutdownController shutdownController;
    private readonly NotifyIcon notifyIcon;
    private bool disposed;

    public TrayApplicationContext()
    {
        runtime = CreateRuntime();
        runtimeCommandController = new RuntimeCommandController(runtime);
        statusItem = new ToolStripMenuItem { Enabled = false };
        hotkeyStatusItem = new ToolStripMenuItem { Enabled = false };
        enableItem = new ToolStripMenuItem("Enable remapping");
        disableItem = new ToolStripMenuItem("Disable remapping");
        cursorLockItem = new ToolStripMenuItem("Lock cursor to target")
        {
            CheckOnClick = true,
        };
        cursorLockController = new TrayCursorLockController(runtime, UpdateRuntimeStatus);

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
        shutdownController = new TrayShutdownController(runtime, HideNotifyIcon, ExitThread);
        hotkeyController = new TrayHotkeyController(
            new WindowsHotkeyRegistrar(),
            runtimeCommandController,
            UpdateRuntimeStatus);
        hotkeyReceiver = new TrayHotkeyReceiver(hotkeyController.DispatchHotkey);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = CreateContextMenu(),
            Icon = SystemIcons.Application,
            Visible = true,
        };

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
                new ToolStripSeparator(),
                exitItem,
            },
        };
    }

    private void UpdateRuntimeStatus()
    {
        RuntimeRemappingStatus status = runtime.Status;
        notifyIcon.Text = TrayStatusFormatter.CreateTrayText(status);
        statusItem.Text = TrayStatusFormatter.CreateRuntimeStatusText(status);
        hotkeyStatusItem.Text = TrayStatusFormatter.CreateHotkeyStatusText(
            hotkeyController.RegistrationResult,
            hotkeyController.LastDispatchedCommand,
            hotkeyController.LastReceivedHotkeyId);
        enableItem.Enabled = status.State is RuntimeRemappingState.Disabled or RuntimeRemappingState.Failed;
        disableItem.Enabled = status.State == RuntimeRemappingState.Enabled;
        cursorLockItem.Checked = runtime.IsCursorLockEnabled;
        cursorLockItem.Enabled = status.State != RuntimeRemappingState.Unsupported;
    }

    private void HideNotifyIcon()
    {
        notifyIcon.Visible = false;
    }

    private static AbsoluteCursorRemappingCoordinator CreateRuntime()
    {
        return new AbsoluteCursorRemappingCoordinator(
            RuntimeProofOfConceptDefaults.CreateOptions(),
            new RawInputMouseMovementSource(),
            new TargetWindowReader(),
            new WindowsCursorPositionController());
    }
}
