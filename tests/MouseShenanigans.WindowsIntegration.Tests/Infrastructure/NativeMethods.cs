using System.Runtime.InteropServices;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static partial class NativeMethods
{
    public const uint DesktopSwitchDesktop = 0x0100;

    public const uint InputKeyboard = 1;

    public const uint KeyEventKeyUp = 0x0002;

    public const ushort VirtualKeyControl = 0x11;

    public const ushort VirtualKeyAlt = 0x12;

    public const ushort VirtualKeyF8 = 0x77;

    public const ushort VirtualKeyF9 = 0x78;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseDesktop(IntPtr hDesktop);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint cInputs, Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        public uint Type;

        public InputUnion Union;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput MouseInput;

        [FieldOffset(0)]
        public KeyboardInput KeyboardInput;

        [FieldOffset(0)]
        public HardwareInput HardwareInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int X;

        public int Y;

        public uint MouseData;

        public uint Flags;

        public uint Time;

        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort VirtualKey;

        public ushort ScanCode;

        public uint Flags;

        public uint Time;

        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint Message;

        public ushort LowParam;

        public ushort HighParam;
    }
}
