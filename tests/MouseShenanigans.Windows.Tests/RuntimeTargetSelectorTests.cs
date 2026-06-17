using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeTargetSelectorTests
{
    [Fact]
    public void EvaluateReturnsInsideBoundsForForegroundProcessMatchWithCursorInsideBounds()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "targetapp",
                title: "Target App",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110)),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));

        RuntimeTargetEligibility eligibility = selector.Evaluate(snapshot);

        Assert.Equal(RuntimeTargetEligibilityState.InsideBounds, eligibility.State);
        Assert.True(eligibility.IsEligibleForRemapping);
    }

    [Fact]
    public void EvaluateReturnsInsideBoundsForWindowTitleMatchUnderCursor()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForWindowTitleContains("target canvas");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: null,
            windowUnderCursor: new TargetWindowInfo(
                processName: "Example",
                title: "Streaming Target Canvas",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110)),
            cursorPosition: new ScreenPoint(50, 50));

        RuntimeTargetEligibility eligibility = selector.Evaluate(snapshot);

        Assert.Equal(RuntimeTargetEligibilityState.InsideBounds, eligibility.State);
    }

    [Fact]
    public void EvaluateReturnsOutsideBoundsForForegroundMatchWithCursorOutsideBounds()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "TargetApp",
                title: "Target App",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110)),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(200, 50));

        RuntimeTargetEligibility eligibility = selector.Evaluate(snapshot);

        Assert.Equal(RuntimeTargetEligibilityState.OutsideBounds, eligibility.State);
        Assert.False(eligibility.IsEligibleForRemapping);
    }

    [Fact]
    public void EvaluateReturnsBoundsUnavailableForMatchWithoutReadableBounds()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: "TargetApp", title: "Target App"),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));

        RuntimeTargetEligibility eligibility = selector.Evaluate(snapshot);

        Assert.Equal(RuntimeTargetEligibilityState.BoundsUnavailable, eligibility.State);
        Assert.False(eligibility.IsEligibleForRemapping);
    }

    [Fact]
    public void EvaluateReturnsNoMatchWhenNeitherForegroundNorUnderCursorWindowMatches()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForProcessName("TargetApp");
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: "OtherApp", title: "Target App"),
            windowUnderCursor: new TargetWindowInfo(processName: "Example", title: "Target Canvas"),
            cursorPosition: new ScreenPoint(50, 50));

        RuntimeTargetEligibility eligibility = selector.Evaluate(snapshot);

        Assert.Equal(RuntimeTargetEligibilityState.NoMatch, eligibility.State);
    }

    [Fact]
    public void ProcessTargetWithExecutablePathRequiresMatchingPathWhenAvailable()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.ForApplicationIdentity(
            new ApplicationIdentity("TargetApp", @"C:\Apps\TargetApp.exe"));
        var matchingSnapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "TargetApp",
                title: "Target App",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110),
                executablePath: @"C:\Apps\TargetApp.exe"),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));
        var wrongPathSnapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "TargetApp",
                title: "Target App",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110),
                executablePath: @"C:\Other\TargetApp.exe"),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));

        Assert.True(selector.Evaluate(matchingSnapshot).IsEligibleForRemapping);
        Assert.Equal(RuntimeTargetEligibilityState.NoMatch, selector.Evaluate(wrongPathSnapshot).State);
    }

    [Fact]
    public void ProcessAndTitleSelectorMatchesEitherProcessOrTitle()
    {
        RuntimeTargetSelector selector = RuntimeTargetSelector.Create(
            "TargetApp",
            executablePath: null,
            windowTitleContains: "Target Canvas");
        var titleOnlySnapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "OtherApp",
                title: "Streaming Target Canvas",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110)),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));
        var processSnapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(
                processName: "TargetApp",
                title: "Other Window",
                bounds: new ScreenRectangle(left: 10, top: 10, right: 110, bottom: 110)),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(50, 50));

        Assert.True(selector.Evaluate(titleOnlySnapshot).IsEligibleForRemapping);
        Assert.True(selector.Evaluate(processSnapshot).IsEligibleForRemapping);
    }
}
