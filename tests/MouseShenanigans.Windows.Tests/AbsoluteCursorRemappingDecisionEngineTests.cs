using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class AbsoluteCursorRemappingDecisionEngineTests
{
    [Fact]
    public void DecideMovesCursorToAbsoluteRemappedPositionForHorizontalInversion()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(
            RuntimeProofOfConceptDefaults.HorizontalInversionProfile);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: true,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 50));

        Assert.Equal(new ScreenPoint(95, 50), decision.TargetPosition);
    }


    [Fact]
    public void DecideScalesAbsoluteCorrectionForMouseDpiCalibration()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(
            RuntimeProofOfConceptDefaults.HorizontalInversionProfile,
            absoluteCorrectionScale: 0.5);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: true,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 50));

        Assert.Equal(new ScreenPoint(100, 50), decision.TargetPosition);
    }

    [Fact]
    public void DecideKeepsCursorWhereItAlreadyIsWhenProfilePreservesMovement()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(
            RuntimeProofOfConceptDefaults.HorizontalInversionProfile);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 0, dy: 5),
            isEnabled: true,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 100));

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
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: true,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 50));

        Assert.Equal(new ScreenPoint(100, 50), decision.TargetPosition);
    }

    [Fact]
    public void DecidePassesThroughWhenDisabledOrTargetDoesNotMatch()
    {
        var engine = new AbsoluteCursorRemappingDecisionEngine(
            RuntimeProofOfConceptDefaults.HorizontalInversionProfile);

        Assert.Null(engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: false,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 50)).TargetPosition);

        Assert.Null(engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: true,
            targetMatches: false,
            anchorPosition: new ScreenPoint(100, 50)).TargetPosition);
    }
}
