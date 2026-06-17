using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public static class DefaultRuntimeHotkeyBindings
{
    public static IReadOnlyList<HotkeyBinding> All { get; } =
    [
        new HotkeyBinding(
            RuntimeCommand.ToggleRuntime,
            HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
            Keys.F8),
        new HotkeyBinding(
            RuntimeCommand.EmergencyDisable,
            HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat,
            Keys.F8),
        new HotkeyBinding(
            RuntimeCommand.CaptureForegroundTarget,
            HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
            Keys.F9),
        new HotkeyBinding(
            RuntimeCommand.CaptureForegroundAllowedApplication,
            HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat,
            Keys.F9),
    ];
}
