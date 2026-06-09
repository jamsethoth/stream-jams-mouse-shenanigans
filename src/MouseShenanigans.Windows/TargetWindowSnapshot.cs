namespace MouseShenanigans.Windows;

public sealed record TargetWindowSnapshot
{
    public TargetWindowSnapshot(
        TargetWindowInfo? foregroundWindow,
        TargetWindowInfo? windowUnderCursor,
        ScreenPoint? cursorPosition = null)
    {
        ForegroundWindow = foregroundWindow;
        WindowUnderCursor = windowUnderCursor;
        CursorPosition = cursorPosition;
    }

    public TargetWindowInfo? ForegroundWindow { get; }

    public TargetWindowInfo? WindowUnderCursor { get; }

    public ScreenPoint? CursorPosition { get; }

    public static TargetWindowSnapshot Empty { get; } = new(
        foregroundWindow: null,
        windowUnderCursor: null);
}
