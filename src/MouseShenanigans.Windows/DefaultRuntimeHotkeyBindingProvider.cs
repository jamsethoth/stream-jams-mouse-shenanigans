using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public sealed class DefaultRuntimeHotkeyBindingProvider : IHotkeyBindingProvider
{
    public static DefaultRuntimeHotkeyBindingProvider Instance { get; } = new();

    private DefaultRuntimeHotkeyBindingProvider()
    {
    }

    public IReadOnlyList<HotkeyBinding> GetBindings()
    {
        return
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
}
