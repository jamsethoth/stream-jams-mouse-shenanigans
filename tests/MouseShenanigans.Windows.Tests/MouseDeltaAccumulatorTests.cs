using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class MouseDeltaAccumulatorTests
{
    [Fact]
    public void ConvertAccumulatesFractionalMovementUntilItReachesWholeInput()
    {
        var accumulator = new MouseDeltaAccumulator();

        Assert.Equal(IntegerMouseDelta.Zero, accumulator.Convert(new RemappedMouseDelta(0.5, -0.5)));
        Assert.Equal(new IntegerMouseDelta(1, -1), accumulator.Convert(new RemappedMouseDelta(0.5, -0.5)));
    }

    [Fact]
    public void ConvertPreservesRemaindersAfterWholeInputIsEmitted()
    {
        var accumulator = new MouseDeltaAccumulator();

        Assert.Equal(new IntegerMouseDelta(1, -1), accumulator.Convert(new RemappedMouseDelta(1.25, -1.25)));
        Assert.Equal(IntegerMouseDelta.Zero, accumulator.Convert(new RemappedMouseDelta(0.25, -0.25)));
        Assert.Equal(new IntegerMouseDelta(1, -1), accumulator.Convert(new RemappedMouseDelta(0.5, -0.5)));
    }
}
