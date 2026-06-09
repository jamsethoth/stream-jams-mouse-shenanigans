using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed record RuntimeRemappingOptions
{
    public RuntimeRemappingOptions(
        RuntimeTargetSelector targetSelector,
        RemappingProfile activeProfile,
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

        TargetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        ActiveProfile = activeProfile ?? throw new ArgumentNullException(nameof(activeProfile));
        AbsoluteCorrectionScale = absoluteCorrectionScale;
    }

    public RuntimeTargetSelector TargetSelector { get; }

    public RemappingProfile ActiveProfile { get; }

    public double AbsoluteCorrectionScale { get; }
}
