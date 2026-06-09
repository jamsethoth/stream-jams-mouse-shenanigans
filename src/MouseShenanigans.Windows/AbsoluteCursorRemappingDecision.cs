namespace MouseShenanigans.Windows;

public readonly record struct AbsoluteCursorRemappingDecision(ScreenPoint? TargetPosition)
{
    public static AbsoluteCursorRemappingDecision PassThrough { get; } = new(TargetPosition: null);

    public static AbsoluteCursorRemappingDecision MoveByCorrection(ScreenPoint currentPosition, IntegerMouseDelta correction)
    {
        return correction.IsZero
            ? PassThrough
            : new AbsoluteCursorRemappingDecision(currentPosition.Offset(correction));
    }
}
