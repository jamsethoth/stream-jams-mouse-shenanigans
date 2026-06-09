using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class TargetWindowReader : ITargetWindowReader
{
    public TargetWindowSnapshot ReadSnapshot()
    {
        TargetWindowInfo? foregroundWindow = ReadWindow(NativeMethods.GetForegroundWindow());
        TargetWindowInfo? windowUnderCursor = null;

        if (NativeMethods.GetCursorPos(out NativePoint cursorPoint))
        {
            windowUnderCursor = ReadWindow(NativeMethods.WindowFromPoint(cursorPoint));
        }

        return new TargetWindowSnapshot(foregroundWindow, windowUnderCursor);
    }

    private static TargetWindowInfo? ReadWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        return new TargetWindowInfo(ReadProcessName(windowHandle), ReadWindowTitle(windowHandle));
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

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;

        public int Y;
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
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, char[] text, int maxCount);
    }
}
