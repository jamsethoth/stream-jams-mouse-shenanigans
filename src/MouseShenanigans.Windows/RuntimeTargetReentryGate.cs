namespace MouseShenanigans.Windows;

public sealed class RuntimeTargetReentryGate
{
    private readonly TimeSpan gracePeriod;
    private readonly IRuntimeClock clock;
    private bool hasObservedState;
    private bool wasInsideBounds;
    private DateTimeOffset? reentryStartedAt;

    public RuntimeTargetReentryGate(TimeSpan gracePeriod, IRuntimeClock clock)
    {
        if (gracePeriod < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(gracePeriod), gracePeriod, "Re-entry grace period must not be negative.");
        }

        this.gracePeriod = gracePeriod;
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public RuntimeTargetReadiness Evaluate(RuntimeTargetEligibility eligibility)
    {
        ArgumentNullException.ThrowIfNull(eligibility);

        if (eligibility.State != RuntimeTargetEligibilityState.InsideBounds)
        {
            hasObservedState = true;
            wasInsideBounds = false;
            reentryStartedAt = null;
            return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: false);
        }

        if (!hasObservedState)
        {
            hasObservedState = true;
            wasInsideBounds = true;
            return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: true);
        }

        if (!wasInsideBounds)
        {
            wasInsideBounds = true;
            reentryStartedAt = clock.UtcNow;
            return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: gracePeriod == TimeSpan.Zero);
        }

        if (reentryStartedAt is not { } startedAt)
        {
            return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: true);
        }

        if (clock.UtcNow - startedAt < gracePeriod)
        {
            return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: false);
        }

        reentryStartedAt = null;
        return new RuntimeTargetReadiness(eligibility, IsEligibleForRemapping: true);
    }

    public void Reset()
    {
        hasObservedState = false;
        wasInsideBounds = false;
        reentryStartedAt = null;
    }
}
