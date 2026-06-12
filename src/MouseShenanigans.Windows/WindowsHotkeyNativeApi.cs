using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed partial class WindowsHotkeyNativeApi : IWindowsHotkeyNativeApi
{
    public bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKeyCode)
    {
        return NativeMethods.RegisterHotKey(windowHandle, id, modifiers, virtualKeyCode);
    }

    public bool UnregisterHotKey(IntPtr windowHandle, int id)
    {
        return NativeMethods.UnregisterHotKey(windowHandle, id);
    }

    public int GetLastError()
    {
        return Marshal.GetLastWin32Error();
    }

    private static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
