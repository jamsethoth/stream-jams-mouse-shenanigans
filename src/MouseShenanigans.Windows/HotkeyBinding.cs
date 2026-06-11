using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public sealed record HotkeyBinding(RuntimeCommand Command, HotkeyModifiers Modifiers, Keys Key);
