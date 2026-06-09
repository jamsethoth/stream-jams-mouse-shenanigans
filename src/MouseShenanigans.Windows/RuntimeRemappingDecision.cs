namespace MouseShenanigans.Windows;

public readonly record struct RuntimeRemappingDecision(
    bool SuppressOriginalMovement,
    IntegerMouseDelta? InjectedMovement)
{
    public static RuntimeRemappingDecision PassThrough { get; } = new(
        SuppressOriginalMovement: false,
        InjectedMovement: null);

    public static RuntimeRemappingDecision SuppressWithoutInjection { get; } = new(
        SuppressOriginalMovement: true,
        InjectedMovement: null);

    public static RuntimeRemappingDecision SuppressAndInject(IntegerMouseDelta movement)
    {
        return movement.IsZero
            ? SuppressWithoutInjection
            : new RuntimeRemappingDecision(SuppressOriginalMovement: true, InjectedMovement: movement);
    }
}
