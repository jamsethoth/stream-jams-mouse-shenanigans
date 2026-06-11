namespace MouseShenanigans.Windows;

public sealed record HotkeyRegistrationResult(
    IReadOnlyList<HotkeyBinding> RegisteredBindings,
    IReadOnlyList<HotkeyRegistrationFailure> Failures)
{
    public static HotkeyRegistrationResult Success { get; } = new([], []);

    public bool Succeeded => Failures.Count == 0;

    public static HotkeyRegistrationResult FromFailures(IReadOnlyList<HotkeyRegistrationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        return new HotkeyRegistrationResult([], failures);
    }
}
