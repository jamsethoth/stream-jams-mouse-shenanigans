namespace MouseShenanigans.Windows;

public interface IWindowsHotkeyNativeApi
{
    bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKeyCode);

    bool UnregisterHotKey(IntPtr windowHandle, int id);

    int GetLastError();
}
