using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MouseShenanigans.Windows;

public sealed class SendInputMouseMovementInjector : IMouseMovementInjector
{
    private const uint InputMouse = 0;
    private const uint MouseEventMove = 0x0001;

    public void Inject(IntegerMouseDelta movement)
    {
        if (movement.IsZero)
        {
            return;
        }

        var input = new NativeInput
        {
            Type = InputMouse,
            MouseInput = new NativeMouseInput
            {
                Dx = movement.Dx,
                Dy = movement.Dy,
                MouseData = 0,
                Flags = MouseEventMove,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            },
        };

        uint sent = NativeMethods.SendInput(1, [input], Marshal.SizeOf<NativeInput>());
        if (sent != 1)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to inject relative mouse movement.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeInput
    {
        public uint Type;

        public NativeMouseInput MouseInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMouseInput
    {
        public int Dx;

        public int Dy;

        public uint MouseData;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }

    private static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint inputCount, NativeInput[] inputs, int inputSize);
    }
}
