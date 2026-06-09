using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AbsoluteCursorRemappingCoordinator runtime;
    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem enableItem;
    private readonly ToolStripMenuItem disableItem;
    private readonly ToolStripMenuItem cursorLockItem;
    private readonly TrayCursorLockController cursorLockController;
    private readonly TrayShutdownController shutdownController;
    private readonly NotifyIcon notifyIcon;
    private bool disposed;

    public TrayApplicationContext()
    {
        runtime = CreateRuntime();
        statusItem = new ToolStripMenuItem { Enabled = false };
        enableItem = new ToolStripMenuItem("Enable remapping");
        disableItem = new ToolStripMenuItem("Disable remapping");
        cursorLockItem = new ToolStripMenuItem("Lock cursor to target")
        {
            CheckOnClick = true,
        };
        cursorLockController = new TrayCursorLockController(runtime, UpdateRuntimeStatus);

        enableItem.Click += (_, _) =>
        {
            runtime.Enable();
            UpdateRuntimeStatus();
        };
        disableItem.Click += (_, _) =>
        {
            runtime.Disable();
            UpdateRuntimeStatus();
        };
        cursorLockItem.Click += (_, _) => cursorLockController.SetCursorLockEnabled(cursorLockItem.Checked);
        shutdownController = new TrayShutdownController(runtime, HideNotifyIcon, ExitThread);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = CreateContextMenu(),
            Icon = SystemIcons.Application,
            Visible = true,
        };

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
        notifyIcon.Text = CreateTrayText(status);
        statusItem.Text = CreateStatusText(status);
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

    private static string CreateTrayText(RuntimeRemappingStatus status)
    {
        return status.State switch
        {
            RuntimeRemappingState.Enabled => "Mouse Shenanigans - enabled",
            RuntimeRemappingState.Unsupported => "Mouse Shenanigans - unsupported",
            RuntimeRemappingState.Failed => "Mouse Shenanigans - failed",
            _ => "Mouse Shenanigans - disabled",
        };
    }

    private static string CreateStatusText(RuntimeRemappingStatus status)
    {
        string stateText = status.State switch
        {
            RuntimeRemappingState.Enabled => $"Enabled for {RuntimeProofOfConceptDefaults.TargetProcessName}",
            RuntimeRemappingState.Unsupported => "Unsupported desktop session",
            RuntimeRemappingState.Failed => "Runtime failed",
            _ => $"Disabled for {RuntimeProofOfConceptDefaults.TargetProcessName}",
        };

        return string.IsNullOrWhiteSpace(status.Message)
            ? stateText
            : $"{stateText}: {status.Message}";
    }
}
