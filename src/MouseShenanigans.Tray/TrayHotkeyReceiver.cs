namespace MouseShenanigans.Tray;

public sealed class TrayHotkeyReceiver : Form
{
    private const int WmHotkey = 0x0312;

    private readonly Func<int, bool> dispatchHotkey;

    public TrayHotkeyReceiver(Func<int, bool> dispatchHotkey)
    {
        this.dispatchHotkey = dispatchHotkey ?? throw new ArgumentNullException(nameof(dispatchHotkey));
        Text = "Mouse Shenanigans Hotkeys";
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.Manual;
        Size = Size.Empty;
        Location = new Point(-32000, -32000);
    }

    public IntPtr WindowHandle => Handle;

    public bool TryDispatchMessage(int messageId, IntPtr wParam)
    {
        return messageId == WmHotkey && dispatchHotkey(wParam.ToInt32());
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        if (TryDispatchMessage(m.Msg, m.WParam))
        {
            m.Result = IntPtr.Zero;
            return;
        }

        base.WndProc(ref m);
    }
}
