using MouseShenanigans.Core;

namespace MouseShenanigans.Core.Tests;

public sealed class RemappingEngineTests
{
    private static readonly RemappingProfile HorizontalInversionProfile = new(
        "horizontal-inversion",
        left: new MovementVector(1, 0),
        right: new MovementVector(-1, 0),
        up: new MovementVector(0, -1),
        down: new MovementVector(0, 1));

    [Fact]
    public void HorizontalInversionReversesRightwardMovement()
    {
        RemappedMouseDelta output = RemappingEngine.Remap(
            dx: 5,
            dy: 0,
            profile: HorizontalInversionProfile);

        Assert.Equal(-5, output.Dx);
        Assert.Equal(0, output.Dy);
    }

    [Fact]
    public void HorizontalInversionReversesLeftwardMovement()
    {
        RemappedMouseDelta output = RemappingEngine.Remap(
            dx: -7,
            dy: 0,
            profile: HorizontalInversionProfile);

        Assert.Equal(7, output.Dx);
        Assert.Equal(0, output.Dy);
    }

    [Fact]
    public void HorizontalInversionPreservesVerticalMovement()
    {
        RemappedMouseDelta output = RemappingEngine.Remap(
            dx: 0,
            dy: -4,
            profile: HorizontalInversionProfile);

        Assert.Equal(0, output.Dx);
        Assert.Equal(-4, output.Dy);
    }

    [Fact]
    public void RemapPreservesZeroMovement()
    {
        RemappedMouseDelta output = RemappingEngine.Remap(
            dx: 0,
            dy: 0,
            profile: HorizontalInversionProfile);

        Assert.Equal(0, output.Dx);
        Assert.Equal(0, output.Dy);
    }

    [Fact]
    public void RemapAppliesDirectionalScaling()
    {
        RemappingProfile profile = new(
            "scale-right",
            left: new MovementVector(-1, 0),
            right: new MovementVector(2, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));

        RemappedMouseDelta output = RemappingEngine.Remap(dx: 3, dy: 0, profile);

        Assert.Equal(6, output.Dx);
        Assert.Equal(0, output.Dy);
    }

    [Fact]
    public void RemapAppliesAxisSwap()
    {
        RemappingProfile profile = new(
            "right-becomes-down",
            left: new MovementVector(-1, 0),
            right: new MovementVector(0, 1),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));

        RemappedMouseDelta output = RemappingEngine.Remap(dx: 4, dy: 0, profile);

        Assert.Equal(0, output.Dx);
        Assert.Equal(4, output.Dy);
    }

    [Fact]
    public void RemapCombinesDiagonalDirections()
    {
        RemappingProfile profile = new(
            "mixed",
            left: new MovementVector(-1, 0),
            right: new MovementVector(1, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 0.5));

        RemappedMouseDelta output = RemappingEngine.Remap(dx: -2, dy: 6, profile);

        Assert.Equal(-2, output.Dx);
        Assert.Equal(3, output.Dy);
    }
}
