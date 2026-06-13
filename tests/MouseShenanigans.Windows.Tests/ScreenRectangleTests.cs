using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class ScreenRectangleTests
{
    [Fact]
    public void ContainsIncludesLeftAndTopEdges()
    {
        var bounds = new ScreenRectangle(left: 10, top: 20, right: 110, bottom: 120);

        Assert.True(bounds.Contains(new ScreenPoint(10, 20)));
    }

    [Fact]
    public void ContainsExcludesRightAndBottomEdges()
    {
        var bounds = new ScreenRectangle(left: 10, top: 20, right: 110, bottom: 120);

        Assert.False(bounds.Contains(new ScreenPoint(110, 50)));
        Assert.False(bounds.Contains(new ScreenPoint(50, 120)));
    }

    [Fact]
    public void ContainsReturnsFalseForPointOutsideBounds()
    {
        var bounds = new ScreenRectangle(left: 10, top: 20, right: 110, bottom: 120);

        Assert.False(bounds.Contains(new ScreenPoint(9, 50)));
        Assert.False(bounds.Contains(new ScreenPoint(50, 19)));
    }

    [Fact]
    public void ClampReturnsNearestPointInsideExclusiveBounds()
    {
        var bounds = new ScreenRectangle(left: 10, top: 20, right: 110, bottom: 120);

        Assert.Equal(new ScreenPoint(10, 119), bounds.Clamp(new ScreenPoint(0, 130)));
        Assert.Equal(new ScreenPoint(109, 20), bounds.Clamp(new ScreenPoint(120, 10)));
        Assert.Equal(new ScreenPoint(50, 60), bounds.Clamp(new ScreenPoint(50, 60)));
    }
}
