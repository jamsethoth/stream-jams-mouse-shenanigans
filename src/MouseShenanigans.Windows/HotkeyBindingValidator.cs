using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public static class HotkeyBindingValidator
{
    public static void Validate(IEnumerable<HotkeyBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var seenChords = new HashSet<HotkeyChord>();
        foreach (HotkeyBinding binding in bindings)
        {
            if (!Enum.IsDefined(binding.Command))
            {
                throw new ArgumentException($"Unknown runtime command '{binding.Command}'.", nameof(bindings));
            }

            if (binding.Key == Keys.None)
            {
                throw new ArgumentException("Hotkey bindings must specify a key.", nameof(bindings));
            }

            var chord = new HotkeyChord(binding.Modifiers & ~HotkeyModifiers.NoRepeat, binding.Key);
            if (!seenChords.Add(chord))
            {
                throw new ArgumentException($"Duplicate hotkey chord '{chord.Modifiers}+{chord.Key}'.", nameof(bindings));
            }
        }
    }

    private sealed record HotkeyChord(HotkeyModifiers Modifiers, Keys Key);
}
