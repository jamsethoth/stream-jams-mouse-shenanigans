using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed record RuntimeRemappingOptions
{
    public static TimeSpan DefaultTargetReentryGracePeriod { get; } = TimeSpan.FromMilliseconds(250);

    public RuntimeRemappingOptions(
        RuntimeTargetSelector targetSelector,
        RemappingProfile activeProfile,
        double absoluteCorrectionScale = 1.0,
        bool cursorLockEnabled = false,
        TimeSpan? targetReentryGracePeriod = null)
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

        TimeSpan reentryGracePeriod = targetReentryGracePeriod ?? DefaultTargetReentryGracePeriod;
        if (reentryGracePeriod < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetReentryGracePeriod),
                reentryGracePeriod,
                "Target re-entry grace period must not be negative.");
        }

        TargetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        ActiveProfile = activeProfile ?? throw new ArgumentNullException(nameof(activeProfile));
        AbsoluteCorrectionScale = absoluteCorrectionScale;
        CursorLockEnabled = cursorLockEnabled;
        TargetReentryGracePeriod = reentryGracePeriod;
    }

    public RuntimeTargetSelector TargetSelector { get; }

    public RemappingProfile ActiveProfile { get; }

    public double AbsoluteCorrectionScale { get; }

    public bool CursorLockEnabled { get; }

    public TimeSpan TargetReentryGracePeriod { get; }
}
