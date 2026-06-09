using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class AbsoluteCursorRemappingDecisionEngineTests
{
    [Fact]
    public void DecideMovesCursorToAbsoluteRemappedPositionForHorizontalInversion()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: true,
            currentPosition: new ScreenPoint(105, 50));

        Assert.Equal(new ScreenPoint(95, 50), decision.TargetPosition);
    }


    [Fact]
    public void DecideScalesAbsoluteCorrectionForMouseDpiCalibration()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(
            BuiltInRemappingProfiles.HorizontalInversion,
            absoluteCorrectionScale: 0.5);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: true,
            currentPosition: new ScreenPoint(105, 50));

        Assert.Equal(new ScreenPoint(100, 50), decision.TargetPosition);
    }

    [Fact]
    public void DecideKeepsCursorWhereItAlreadyIsWhenProfilePreservesMovement()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 0, dy: 5, isInjected: false),
            isEnabled: true,
            targetMatches: true,
            currentPosition: new ScreenPoint(100, 105));

        Assert.Null(decision.TargetPosition);
    }

    [Fact]
    public void DecideReturnsPreviousPositionWhenRemappedOutputIsZero()
    {
        var zeroRightMovementProfile = new RemappingProfile(
            "zero-right",
            left: new MovementVector(-1, 0),
            right: new MovementVector(0, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));
        var engine = new AbsoluteCursorRemappingDecisionEngine(zeroRightMovementProfile);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: true,
            currentPosition: new ScreenPoint(105, 50));

        Assert.Equal(new ScreenPoint(100, 50), decision.TargetPosition);
    }

    [Fact]
    public void DecidePassesThroughWhenDisabledOrTargetDoesNotMatch()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(BuiltInRemappingProfiles.HorizontalInversion);

        Assert.Null(engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: false,
            targetMatches: true,
            currentPosition: new ScreenPoint(105, 50)).TargetPosition);

        Assert.Null(engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0, isInjected: false),
            isEnabled: true,
            targetMatches: false,
            currentPosition: new ScreenPoint(105, 50)).TargetPosition);
    }
}
