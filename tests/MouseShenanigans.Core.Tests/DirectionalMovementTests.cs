using MouseShenanigans.Core;

namespace MouseShenanigans.Core.Tests;

public sealed class DirectionalMovementTests
{
    [Fact]
    public void FromDeltaDecomposesRightwardHorizontalMovement()
    {
        DirectionalMovement movement = DirectionalMovement.FromDelta(dx: 12.5, dy: 0);

        Assert.Equal(0, movement.Left);
        Assert.Equal(12.5, movement.Right);
        Assert.Equal(0, movement.Up);
        Assert.Equal(0, movement.Down);
    }

    [Fact]
    public void FromDeltaDecomposesLeftwardHorizontalMovement()
    {
        DirectionalMovement movement = DirectionalMovement.FromDelta(dx: -8, dy: 0);

        Assert.Equal(8, movement.Left);
        Assert.Equal(0, movement.Right);
        Assert.Equal(0, movement.Up);
        Assert.Equal(0, movement.Down);
    }

    [Fact]
    public void FromDeltaDecomposesVerticalMovement()
    {
        DirectionalMovement upward = DirectionalMovement.FromDelta(dx: 0, dy: -4);
        DirectionalMovement downward = DirectionalMovement.FromDelta(dx: 0, dy: 6);

        Assert.Equal(4, upward.Up);
        Assert.Equal(0, upward.Down);
        Assert.Equal(0, downward.Up);
        Assert.Equal(6, downward.Down);
    }

    [Fact]
    public void FromDeltaDecomposesDiagonalMovement()
    {
        DirectionalMovement movement = DirectionalMovement.FromDelta(dx: -3, dy: 7);

        Assert.Equal(3, movement.Left);
        Assert.Equal(0, movement.Right);
        Assert.Equal(0, movement.Up);
        Assert.Equal(7, movement.Down);
    }

    [Fact]
    public void FromDeltaDecomposesZeroMovement()
    {
        DirectionalMovement movement = DirectionalMovement.FromDelta(dx: 0, dy: 0);

        Assert.Equal(0, movement.Left);
        Assert.Equal(0, movement.Right);
        Assert.Equal(0, movement.Up);
        Assert.Equal(0, movement.Down);
    }
}
