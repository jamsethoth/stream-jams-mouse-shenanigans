using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public static class TrayStatusFormatter
{
    public static string CreateTrayText(RuntimeRemappingStatus status)
    {
        return status.State switch
        {
            RuntimeRemappingState.Enabled => "Mouse Shenanigans - enabled",
            RuntimeRemappingState.Unsupported => "Mouse Shenanigans - unsupported",
            RuntimeRemappingState.Failed => "Mouse Shenanigans - failed",
            _ => "Mouse Shenanigans - disabled",
        };
    }

    public static string CreateRuntimeStatusText(
        RuntimeRemappingStatus status,
        RuntimeConfiguration? configuration = null,
        string? configurationStatus = null)
    {
        string target = configuration?.TargetDisplayName ?? RuntimeProofOfConceptDefaults.TargetProcessName;
        string profile = configuration?.ActiveProfileName ?? RuntimeProofOfConceptDefaults.ActiveProfileName;
        string stateText = status.State switch
        {
            RuntimeRemappingState.Enabled => $"Enabled for {target} using {profile}",
            RuntimeRemappingState.Unsupported => "Unsupported desktop session",
            RuntimeRemappingState.Failed => "Runtime failed",
            _ => $"Disabled for {target} using {profile}",
        };

        string[] messages = new[] { status.Message, configurationStatus }
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message!)
            .ToArray();

        return messages.Length == 0
            ? stateText
            : $"{stateText}: {string.Join("; ", messages)}";
    }

    public static string CreateHotkeyStatusText(
        HotkeyRegistrationResult registrationResult,
        RuntimeCommand? lastDispatchedCommand = null,
        int? lastReceivedHotkeyId = null)
    {
        if (registrationResult.Succeeded)
        {
            return lastDispatchedCommand is { } command
                ? $"Hotkeys: registered - last {command} (id {lastReceivedHotkeyId})"
                : "Hotkeys: registered - no hotkey received";
        }

        string failureText = registrationResult.Failures.Count == 1
            ? CreateFailureText(registrationResult.Failures[0])
            : $"{registrationResult.Failures.Count} hotkeys failed to register; first {CreateFailureText(registrationResult.Failures[0])}";

        return $"Hotkeys: degraded - {failureText}";
    }

    private static string CreateFailureText(HotkeyRegistrationFailure failure)
    {
        return $"{failure.Binding.Command} {CreateChordText(failure.Binding)}: {failure.Message}";
    }

    private static string CreateChordText(HotkeyBinding binding)
    {
        List<string> parts = [];
        HotkeyModifiers modifiers = binding.Modifiers & ~HotkeyModifiers.NoRepeat;
        if (modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(binding.Key.ToString());
        return string.Join("+", parts);
    }
}
