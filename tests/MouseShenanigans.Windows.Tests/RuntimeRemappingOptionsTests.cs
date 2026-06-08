using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeRemappingOptionsTests
{
    [Fact]
    public void ConstructorStoresAbsoluteCorrectionScale()
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            BuiltInRemappingProfiles.HorizontalInversion,
            absoluteCorrectionScale: 0.75);

        Assert.Equal(0.75, options.AbsoluteCorrectionScale);
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
            BuiltInRemappingProfiles.HorizontalInversion,
            absoluteCorrectionScale));
    }
}
