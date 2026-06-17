using System.Windows.Forms;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeHotkeyBindingTests
{
    [Fact]
    public void DefaultBindingsIncludeToggleEmergencyDisableTargetCaptureAndAllowedApplicationCapture()
    {
        IReadOnlyList<HotkeyBinding> bindings = DefaultRuntimeHotkeyBindings.All;

        Assert.Collection(
            bindings,
            binding =>
            {
                Assert.Equal(RuntimeCommand.ToggleRuntime, binding.Command);
                Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat, binding.Modifiers);
                Assert.Equal(Keys.F8, binding.Key);
            },
            binding =>
            {
                Assert.Equal(RuntimeCommand.EmergencyDisable, binding.Command);
                Assert.Equal(
                    HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat,
                    binding.Modifiers);
                Assert.Equal(Keys.F8, binding.Key);
            },
            binding =>
            {
                Assert.Equal(RuntimeCommand.CaptureForegroundTarget, binding.Command);
                Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat, binding.Modifiers);
                Assert.Equal(Keys.F9, binding.Key);
            },
            binding =>
            {
                Assert.Equal(RuntimeCommand.CaptureForegroundAllowedApplication, binding.Command);
                Assert.Equal(
                    HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat,
                    binding.Modifiers);
                Assert.Equal(Keys.F9, binding.Key);
            });
    }

    [Fact]
    public void DuplicateChordsAreRejectedBeforeRegistration()
    {
        var bindings = new[]
        {
            new HotkeyBinding(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
            new HotkeyBinding(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
        };

        Assert.Throws<ArgumentException>(() => HotkeyBindingValidator.Validate(bindings));
    }

    [Fact]
    public void DuplicateChordsIgnoreNoRepeatModifier()
    {
        var bindings = new[]
        {
            new HotkeyBinding(
                RuntimeCommand.ToggleRuntime,
                HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
                Keys.M),
            new HotkeyBinding(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
        };

        Assert.Throws<ArgumentException>(() => HotkeyBindingValidator.Validate(bindings));
    }

    [Fact]
    public void UnknownRuntimeCommandsAreRejectedBeforeRegistration()
    {
        var bindings = new[]
        {
            new HotkeyBinding((RuntimeCommand)999, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
        };

        Assert.Throws<ArgumentException>(() => HotkeyBindingValidator.Validate(bindings));
    }
}
