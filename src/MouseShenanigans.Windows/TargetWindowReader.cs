using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class TargetWindowReader : ITargetWindowReader
{
    private const uint GetAncestorRoot = 2;
    private const int DwmWindowAttributeExtendedFrameBounds = 9;

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

        windowHandle = NormalizeWindowHandle(windowHandle);

        return new TargetWindowInfo(
            ReadProcessName(windowHandle),
            ReadWindowTitle(windowHandle),
            ReadWindowBounds(windowHandle),
            ReadExecutablePath(windowHandle));
    }

    private static IntPtr NormalizeWindowHandle(IntPtr windowHandle)
    {
        IntPtr rootWindowHandle = NativeMethods.GetAncestor(windowHandle, GetAncestorRoot);
        return rootWindowHandle == IntPtr.Zero ? windowHandle : rootWindowHandle;
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

    private static string? ReadExecutablePath(IntPtr windowHandle)
    {
        _ = NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId);

        if (processId == 0)
        {
            return null;
        }

        try
        {
            using Process process = Process.GetProcessById((int)processId);
            return process.MainModule?.FileName;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotSupportedException)
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
        if (TryReadVisibleWindowBounds(windowHandle, out ScreenRectangle visibleBounds))
        {
            return visibleBounds;
        }

        return TryReadWindowRectBounds(windowHandle, out ScreenRectangle windowRectBounds)
            ? windowRectBounds
            : null;
    }

    private static bool TryReadVisibleWindowBounds(IntPtr windowHandle, out ScreenRectangle bounds)
    {
        bounds = default;
        int result = NativeMethods.DwmGetWindowAttribute(
            windowHandle,
            DwmWindowAttributeExtendedFrameBounds,
            out NativeRectangle rectangle,
            Marshal.SizeOf<NativeRectangle>());

        return result == 0 && TryCreateBounds(rectangle, out bounds);
    }

    private static bool TryReadWindowRectBounds(IntPtr windowHandle, out ScreenRectangle bounds)
    {
        // Fallback for windows where DWM frame bounds are unavailable. Coordinates
        // still match GetCursorPos; right/bottom remain exclusive containment edges.
        if (!NativeMethods.GetWindowRect(windowHandle, out NativeRectangle rectangle))
        {
            bounds = default;
            return false;
        }

        return TryCreateBounds(rectangle, out bounds);
    }

    private static bool TryCreateBounds(NativeRectangle rectangle, out ScreenRectangle bounds)
    {
        if (rectangle.Right <= rectangle.Left || rectangle.Bottom <= rectangle.Top)
        {
            bounds = default;
            return false;
        }

        bounds = new ScreenRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        return true;
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
        internal static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out NativeRectangle rect);

        [DllImport("dwmapi.dll", SetLastError = true)]
        internal static extern int DwmGetWindowAttribute(
            IntPtr hwnd,
            int dwAttribute,
            out NativeRectangle pvAttribute,
            int cbAttribute);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, char[] text, int maxCount);
    }
}
