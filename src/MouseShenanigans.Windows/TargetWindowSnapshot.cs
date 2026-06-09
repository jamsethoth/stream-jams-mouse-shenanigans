namespace MouseShenanigans.Windows;

public sealed record TargetWindowSnapshot
{
    public TargetWindowSnapshot(TargetWindowInfo? foregroundWindow, TargetWindowInfo? windowUnderCursor)
    {
        ForegroundWindow = foregroundWindow;
        WindowUnderCursor = windowUnderCursor;
    }

    public TargetWindowInfo? ForegroundWindow { get; }

    public TargetWindowInfo? WindowUnderCursor { get; }

    public static TargetWindowSnapshot Empty { get; } = new(
        foregroundWindow: null,
        windowUnderCursor: null);
}
