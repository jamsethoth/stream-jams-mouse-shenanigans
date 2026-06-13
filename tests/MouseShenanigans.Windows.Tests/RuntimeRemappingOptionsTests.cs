using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeRemappingOptionsTests
{
    private static readonly RemappingProfile TestProfile = new(
        "test-profile",
        left: new MovementVector(-1, 0),
        right: new MovementVector(1, 0),
        up: new MovementVector(0, -1),
        down: new MovementVector(0, 1));

    [Fact]
    public void ConstructorStoresAbsoluteCorrectionScale()
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            TestProfile,
            absoluteCorrectionScale: 0.75);

        Assert.Equal(0.75, options.AbsoluteCorrectionScale);
    }


    [Fact]
    public void ConstructorDisablesCursorLockByDefault()
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            TestProfile);

        Assert.False(options.CursorLockEnabled);
    }

    [Fact]
    public void ConstructorStoresCursorLockSetting()
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            TestProfile,
            cursorLockEnabled: true);

        Assert.True(options.CursorLockEnabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ConstructorRejectsInvalidAbsoluteCorrectionScale(double absoluteCorrectionScale)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            TestProfile,
            absoluteCorrectionScale));
    }

}
