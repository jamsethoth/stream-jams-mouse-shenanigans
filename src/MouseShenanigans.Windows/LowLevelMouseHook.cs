using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class LowLevelMouseHook : IMouseMovementHook
{
    private const int WhMouseLl = 14;
    private const int WmMouseMove = 0x0200;
    private const int LlMouseInjected = 0x00000001;

    private readonly LowLevelMouseProc hookProc;
    private Func<RuntimeMouseMovement, bool>? onMovement;
    private IntPtr hookHandle;
    private NativePoint? previousPoint;
    private bool disposed;

    public LowLevelMouseHook()
    {
        hookProc = HookCallback;
    }

    public void Start(Func<RuntimeMouseMovement, bool> onMovement)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(onMovement);

        if (hookHandle != IntPtr.Zero)
        {
            return;
        }

        this.onMovement = onMovement;
        previousPoint = null;
        IntPtr moduleHandle = NativeMethods.GetModuleHandle(null);
        hookHandle = NativeMethods.SetWindowsHookEx(WhMouseLl, hookProc, moduleHandle, 0);

        if (hookHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to install the low-level mouse hook.");
        }
    }

    public void StopHook()
    {
        if (hookHandle == IntPtr.Zero)
        {
            return;
        }

        if (!NativeMethods.UnhookWindowsHookEx(hookHandle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to remove the low-level mouse hook.");
        }

        hookHandle = IntPtr.Zero;
        onMovement = null;
        previousPoint = null;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            StopHook();
        }
        catch (Win32Exception)
        {
        }

        disposed = true;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0 || wParam != WmMouseMove || onMovement is null)
        {
            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        bool suppressOriginal = false;

        try
        {
            var hookData = Marshal.PtrToStructure<MouseHookData>(lParam);
            NativePoint currentPoint = hookData.Point;
            bool isInjected = (hookData.Flags & LlMouseInjected) != 0;

            if (previousPoint is { } previous)
            {
                var movement = new RuntimeMouseMovement(
                    dx: currentPoint.X - previous.X,
                    dy: currentPoint.Y - previous.Y,
                    isInjected);
                suppressOriginal = onMovement(movement);
            }

            previousPoint = currentPoint;
        }
        catch
        {
            suppressOriginal = false;
        }

        return suppressOriginal
            ? new IntPtr(1)
            : NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseHookData
    {
        public NativePoint Point;

        public uint MouseData;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }

    private static partial class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string? moduleName);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelMouseProc lpfn,
            IntPtr hmod,
            uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    }
}
