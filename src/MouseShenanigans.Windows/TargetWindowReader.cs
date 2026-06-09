using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class TargetWindowReader : ITargetWindowReader
{
    public TargetWindowSnapshot ReadSnapshot()
    {
        TargetWindowInfo? foregroundWindow = ReadWindow(NativeMethods.GetForegroundWindow());
        TargetWindowInfo? windowUnderCursor = null;
        ScreenPoint? cursorPosition = null;

        if (NativeMethods.GetCursorPos(out NativePoint cursorPoint))
        {
            cursorPosition = new ScreenPoint(cursorPoint.X, cursorPoint.Y);
            windowUnderCursor = ReadWindow(NativeMethods.WindowFromPoint(cursorPoint));
        }

        return new TargetWindowSnapshot(foregroundWindow, windowUnderCursor, cursorPosition);
    }

    private static TargetWindowInfo? ReadWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        return new TargetWindowInfo(
            ReadProcessName(windowHandle),
            ReadWindowTitle(windowHandle),
            ReadWindowBounds(windowHandle));
    }

    private static string? ReadProcessName(IntPtr windowHandle)
    {
        _ = NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId);

        if (processId == 0)
        {
            return null;
        }

        try
        {
            using Process process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private static string? ReadWindowTitle(IntPtr windowHandle)
    {
        int length = NativeMethods.GetWindowTextLength(windowHandle);
        if (length <= 0)
        {
            return null;
        }

        char[] buffer = new char[length + 1];
        int copied = NativeMethods.GetWindowText(windowHandle, buffer, buffer.Length);

        return copied <= 0 ? null : new string(buffer, startIndex: 0, length: copied);
    }

    private static ScreenRectangle? ReadWindowBounds(IntPtr windowHandle)
    {
        // GetWindowRect and GetCursorPos both use screen coordinates; the full window
        // rectangle is kept as-is so right/bottom remain the exclusive containment edges.
        if (!NativeMethods.GetWindowRect(windowHandle, out NativeRectangle rectangle))
        {
            return null;
        }

        return new ScreenRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;

        public int Y;
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
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out NativePoint point);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(NativePoint point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out NativeRectangle rect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, char[] text, int maxCount);
    }
}
