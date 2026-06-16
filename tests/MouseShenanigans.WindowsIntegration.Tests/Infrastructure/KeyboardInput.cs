using System.Runtime.InteropServices;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class KeyboardInput
{
    public static void SendForegroundCaptureHotkey()
    {
        NativeMethods.Input[] inputs =
        [
            KeyDown(NativeMethods.VirtualKeyControl),
            KeyDown(NativeMethods.VirtualKeyAlt),
            KeyDown(NativeMethods.VirtualKeyF9),
            KeyUp(NativeMethods.VirtualKeyF9),
            KeyUp(NativeMethods.VirtualKeyAlt),
            KeyUp(NativeMethods.VirtualKeyControl),
        ];

        uint sent = NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<NativeMethods.Input>());
        if (sent != inputs.Length)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"SendInput sent {sent} of {inputs.Length} foreground capture hotkey events. Win32 error: {errorCode}.");
        }
    }

    private static NativeMethods.Input KeyDown(ushort virtualKey)
    {
        return CreateKeyboardInput(virtualKey, flags: 0);
    }

    private static NativeMethods.Input KeyUp(ushort virtualKey)
    {
        return CreateKeyboardInput(virtualKey, NativeMethods.KeyEventKeyUp);
    }

    private static NativeMethods.Input CreateKeyboardInput(ushort virtualKey, uint flags)
    {
        return new NativeMethods.Input
        {
            Type = NativeMethods.InputKeyboard,
            Union = new NativeMethods.InputUnion
            {
                KeyboardInput = new NativeMethods.KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = flags,
                },
            },
        };
    }
}
