namespace MouseShenanigans.Windows;

public sealed record RuntimeTargetEligibility
{
    private RuntimeTargetEligibility(RuntimeTargetEligibilityState state, TargetWindowInfo? targetWindow)
    {
        State = state;
        TargetWindow = targetWindow;
    }

    public RuntimeTargetEligibilityState State { get; }

    public TargetWindowInfo? TargetWindow { get; }

    public ScreenRectangle? TargetBounds => TargetWindow?.Bounds;

    public bool IsEligibleForRemapping => State == RuntimeTargetEligibilityState.InsideBounds;

    public static RuntimeTargetEligibility NoMatch { get; } = new(RuntimeTargetEligibilityState.NoMatch, targetWindow: null);

    public static RuntimeTargetEligibility InsideBounds(TargetWindowInfo targetWindow)
    {
        ArgumentNullException.ThrowIfNull(targetWindow);

        return new RuntimeTargetEligibility(RuntimeTargetEligibilityState.InsideBounds, targetWindow);
    }

    public static RuntimeTargetEligibility OutsideBounds(TargetWindowInfo targetWindow)
    {
        ArgumentNullException.ThrowIfNull(targetWindow);

        return new RuntimeTargetEligibility(RuntimeTargetEligibilityState.OutsideBounds, targetWindow);
    }

    public static RuntimeTargetEligibility BoundsUnavailable(TargetWindowInfo targetWindow)
    {
        ArgumentNullException.ThrowIfNull(targetWindow);

        return new RuntimeTargetEligibility(RuntimeTargetEligibilityState.BoundsUnavailable, targetWindow);
    }
}
