using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeRemappingDecisionEngineTests
{
    [Fact]
    public void DecidePassesThroughMovementWhenRuntimeIsDisabled()
    {
        var engine = new RuntimeRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        RuntimeRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: false,
            targetMatches: true);

        Assert.False(decision.SuppressOriginalMovement);
        Assert.Null(decision.InjectedMovement);
    }

    [Fact]
    public void DecidePassesThroughMovementWhenTargetDoesNotMatch()
    {
        var engine = new RuntimeRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        RuntimeRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: false);

        Assert.False(decision.SuppressOriginalMovement);
        Assert.Null(decision.InjectedMovement);
    }

    [Fact]
    public void DecideRemapsTargetedPhysicalMovement()
    {
        var engine = new RuntimeRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        RuntimeRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: true);

        Assert.True(decision.SuppressOriginalMovement);
        Assert.Equal(new IntegerMouseDelta(-5, 0), decision.InjectedMovement);
    }

    [Fact]
    public void DecideSuppressesOriginalMovementWhenRemappedOutputIsZero()
    {
        var zeroRightMovementProfile = new RemappingProfile(
            "zero-right",
            left: new MovementVector(-1, 0),
            right: new MovementVector(0, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));
        var engine = new RuntimeRemappingDecisionEngine(zeroRightMovementProfile);

        RuntimeRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: true);

        Assert.True(decision.SuppressOriginalMovement);
        Assert.Null(decision.InjectedMovement);
    }

    [Fact]
    public void DecidePassesThroughInjectedMovementWithoutRemappingItAgain()
    {
        var engine = new RuntimeRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        RuntimeRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: -5, dy: 0, isInjected: true),
            isEnabled: true,
            targetMatches: true);

        Assert.False(decision.SuppressOriginalMovement);
        Assert.Null(decision.InjectedMovement);
    }
}
