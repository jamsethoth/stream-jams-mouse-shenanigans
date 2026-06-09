using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed class AbsoluteCursorRemappingDecisionEngine
{
    private readonly RemappingProfile profile;
    private readonly MouseDeltaAccumulator accumulator;
    private readonly double absoluteCorrectionScale;

    public AbsoluteCursorRemappingDecisionEngine(RemappingProfile profile, double absoluteCorrectionScale = 1.0)
        : this(profile, new MouseDeltaAccumulator(), absoluteCorrectionScale)
    {
    }

    public AbsoluteCursorRemappingDecisionEngine(
        RemappingProfile profile,
        MouseDeltaAccumulator accumulator,
        double absoluteCorrectionScale = 1.0)
    {
        if (absoluteCorrectionScale <= 0
            || double.IsNaN(absoluteCorrectionScale)
            || double.IsInfinity(absoluteCorrectionScale))
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteCorrectionScale),
                absoluteCorrectionScale,
                "Absolute correction scale must be a finite positive value.");
        }

        this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
        this.accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
        this.absoluteCorrectionScale = absoluteCorrectionScale;
    }

    public AbsoluteCursorRemappingDecision Decide(
        RuntimeMouseMovement movement,
        bool isEnabled,
        bool targetMatches,
        ScreenPoint currentPosition)
    {
        if (!isEnabled || !targetMatches || movement.IsInjected)
        {
            return AbsoluteCursorRemappingDecision.PassThrough;
        }

        RemappedMouseDelta remapped = RemappingEngine.Remap(movement.Dx, movement.Dy, profile);
        var correction = new RemappedMouseDelta(
            (remapped.Dx - movement.Dx) * absoluteCorrectionScale,
            (remapped.Dy - movement.Dy) * absoluteCorrectionScale);
        IntegerMouseDelta integerCorrection = accumulator.Convert(correction);

        return AbsoluteCursorRemappingDecision.MoveByCorrection(currentPosition, integerCorrection);
    }

    public void ResetAccumulator()
    {
        accumulator.Reset();
    }
}
