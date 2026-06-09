using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class WindowsCursorLockController : ICursorLockController
{
    public void LockTo(ScreenRectangle bounds)
    {
        var rectangle = new NativeRectangle
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Right = bounds.Right,
            Bottom = bounds.Bottom,
        };

        if (!NativeMethods.ClipCursor(ref rectangle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public void Release()
    {
        if (!NativeMethods.ClipCursor(IntPtr.Zero))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    private static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ClipCursor(ref NativeRectangle rect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ClipCursor(IntPtr rect);
    }
}
