using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed class RuntimeRemappingDecisionEngine
{
    private readonly RemappingProfile profile;
    private readonly MouseDeltaAccumulator accumulator;

    public RuntimeRemappingDecisionEngine(RemappingProfile profile)
        : this(profile, new MouseDeltaAccumulator())
    {
    }

    public RuntimeRemappingDecisionEngine(RemappingProfile profile, MouseDeltaAccumulator accumulator)
    {
        this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
        this.accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
    }

    public RuntimeRemappingDecision Decide(RuntimeMouseMovement movement, bool isEnabled, bool targetMatches)
    {
        if (!isEnabled || !targetMatches || movement.IsInjected)
        {
            return RuntimeRemappingDecision.PassThrough;
        }

        RemappedMouseDelta remapped = RemappingEngine.Remap(movement.Dx, movement.Dy, profile);
        IntegerMouseDelta injection = accumulator.Convert(remapped);

        return RuntimeRemappingDecision.SuppressAndInject(injection);
    }

    public void ResetAccumulator()
    {
        accumulator.Reset();
    }
}
