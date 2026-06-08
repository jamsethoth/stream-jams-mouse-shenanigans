using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public sealed class RawInputMouseMovementSource : NativeWindow, IRawMouseMovementSource
{
    private const int WmInput = 0x00FF;
    private const uint RawInputTypeMouse = 0;
    private const uint RidInput = 0x10000003;
    private const uint RidevRemove = 0x00000001;
    private const uint RidevInputSink = 0x00000100;
    private const ushort UsagePageGenericDesktop = 0x01;
    private const ushort UsageMouse = 0x02;
    private const ushort RawMouseMoveAbsolute = 0x0001;

    private Action<IntegerMouseDelta>? onMovement;
    private bool isStarted;
    private bool disposed;

    public void Start(Action<IntegerMouseDelta> onMovement)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(onMovement);

        if (isStarted)
        {
            return;
        }

        this.onMovement = onMovement;
        CreateHandle(new CreateParams { Caption = "Mouse Shenanigans Raw Input" });
        RegisterRawMouseInput(RidevInputSink, Handle);
        isStarted = true;
    }

    public void StopObservation()
    {
        if (!isStarted)
        {
            return;
        }

        try
        {
            RegisterRawMouseInput(RidevRemove, IntPtr.Zero);
        }
        finally
        {
            isStarted = false;
            onMovement = null;

            if (Handle != IntPtr.Zero)
            {
                DestroyHandle();
            }
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            StopObservation();
        }
        catch (Win32Exception)
        {
        }

        disposed = true;
        GC.SuppressFinalize(this);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmInput
            && onMovement is { } callback
            && TryReadRelativeRawMouseMovement(m.LParam, out IntegerMouseDelta movement)
            && !movement.IsZero)
        {
            callback(movement);
        }

        base.WndProc(ref m);
    }

    private static bool TryReadRelativeRawMouseMovement(IntPtr rawInputHandle, out IntegerMouseDelta movement)
    {
        movement = IntegerMouseDelta.Zero;
        uint size = 0;
        uint headerSize = (uint)Marshal.SizeOf<RawInputHeader>();
        uint result = NativeMethods.GetRawInputData(rawInputHandle, RidInput, IntPtr.Zero, ref size, headerSize);

        if (result == uint.MaxValue || size == 0)
        {
            return false;
        }

        IntPtr buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            uint read = NativeMethods.GetRawInputData(rawInputHandle, RidInput, buffer, ref size, headerSize);
            if (read == uint.MaxValue || read != size)
            {
                return false;
            }

            RawInput rawInput = Marshal.PtrToStructure<RawInput>(buffer);
            if (rawInput.Header.Type != RawInputTypeMouse
                || (rawInput.Mouse.Flags & RawMouseMoveAbsolute) != 0)
            {
                return false;
            }

            movement = new IntegerMouseDelta(rawInput.Mouse.LastX, rawInput.Mouse.LastY);
            return true;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void RegisterRawMouseInput(uint flags, IntPtr target)
    {
        var device = new RawInputDevice
        {
            UsagePage = UsagePageGenericDesktop,
            Usage = UsageMouse,
            Flags = flags,
            Target = target,
        };

        if (!NativeMethods.RegisterRawInputDevices([device], 1, (uint)Marshal.SizeOf<RawInputDevice>()))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register Raw Input mouse observation.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputDevice
    {
        public ushort UsagePage;

        public ushort Usage;

        public uint Flags;

        public IntPtr Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputHeader
    {
        public uint Type;

        public uint Size;

        public IntPtr Device;

        public IntPtr WParam;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct RawMouse
    {
        [FieldOffset(0)]
        public ushort Flags;

        [FieldOffset(4)]
        public ushort ButtonFlags;

        [FieldOffset(6)]
        public ushort ButtonData;

        [FieldOffset(8)]
        public uint RawButtons;

        [FieldOffset(12)]
        public int LastX;

        [FieldOffset(16)]
        public int LastY;

        [FieldOffset(20)]
        public uint ExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInput
    {
        public RawInputHeader Header;

        public RawMouse Mouse;
    }

    private static partial class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterRawInputDevices(
            RawInputDevice[] rawInputDevices,
            uint deviceCount,
            uint rawInputDeviceSize);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputData(
            IntPtr rawInput,
            uint command,
            IntPtr data,
            ref uint size,
            uint headerSize);
    }
}
