using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeTargetSelectorTests
{
    [Fact]
    public void IsMatchMatchesProcessNameIgnoringCaseForForegroundWindow()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: "targetapp", title: "Target App"),
            windowUnderCursor: null);

        Assert.True(selector.IsMatch(snapshot));
    }

    [Fact]
    public void IsMatchMatchesWindowTitleIgnoringCaseForWindowUnderCursor()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForWindowTitleContains("target canvas");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: null,
            windowUnderCursor: new TargetWindowInfo(processName: "Example", title: "Streaming Target Canvas"));

        Assert.True(selector.IsMatch(snapshot));
    }

    [Fact]
    public void IsMatchReturnsFalseWhenNeitherForegroundNorUnderCursorWindowMatches()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: "OtherApp", title: "Target App"),
            windowUnderCursor: new TargetWindowInfo(processName: "Example", title: "Target Canvas"));

        Assert.False(selector.IsMatch(snapshot));
    }
}
