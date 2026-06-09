using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class WindowsCursorPositionController : ICursorPositionController
{
    public ScreenPoint GetPosition()
    {
        if (!NativeMethods.GetCursorPos(out NativePoint point))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to read the cursor position.");
        }

        return new ScreenPoint(point.X, point.Y);
    }

    public void SetPosition(ScreenPoint targetPosition)
    {
        if (!NativeMethods.SetCursorPos(targetPosition.X, targetPosition.Y))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set the cursor position.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;

        public int Y;
    }

    private static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out NativePoint point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int x, int y);
    }
}
