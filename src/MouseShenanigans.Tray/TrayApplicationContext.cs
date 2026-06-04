using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon notifyIcon;

    public TrayApplicationContext()
    {
        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = CreateContextMenu(),
            Icon = SystemIcons.Application,
            Text = CreateTrayText(),
            Visible = true,
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private static ContextMenuStrip CreateContextMenu()
    {
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Application.Exit();

        return new ContextMenuStrip
        {
            Items = { exitItem },
        };
    }

    private static string CreateTrayText()
    {
        return WindowsRuntime.IsDesktopInputAvailable
            ? "Mouse Shenanigans"
            : "Mouse Shenanigans (unsupported platform)";
    }
}
