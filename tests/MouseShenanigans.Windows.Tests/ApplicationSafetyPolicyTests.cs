using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class ApplicationSafetyPolicyTests
{
    [Fact]
    public void EmptyAllowlistDeniesGameRuntimeEnable()
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: ["TargetApp"]);
        RuntimeConfiguration configuration = CreateConfiguration(safety);

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.False(decision.Allowed);
        Assert.Equal(ApplicationSafetyDenialReason.AllowlistEmpty, decision.DenialReason);
        Assert.Contains("not allowlisted", decision.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TargetApp", decision.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MatchingAllowlistEntryAllowsRuntimeEnable()
    {
        var safety = new ApplicationSafetyConfiguration(
            allowedApplications:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("TargetApp")),
            ]);
        RuntimeConfiguration configuration = CreateConfiguration(safety);

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.True(decision.Allowed);
        Assert.NotNull(decision.MatchedAllowlistEntry);
        Assert.Contains("TargetApp.exe", decision.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ProtectedDenyRuleTakesPrecedenceOverAllowlist()
    {
        var safety = new ApplicationSafetyConfiguration(
            allowedApplications:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("TargetApp"), "User allowlist"),
            ],
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("TargetApp"), "Protected fixture"),
            ]);
        RuntimeConfiguration configuration = CreateConfiguration(safety);

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.False(decision.Allowed);
        Assert.Equal(ApplicationSafetyDenialReason.ProtectedGameDenyRule, decision.DenialReason);
        Assert.Equal("Protected fixture", decision.MatchedProtectedDenyRule?.Label);
        Assert.Contains("Protected fixture", decision.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NonGameUtilityTargetDoesNotRequireAllowlistEntry()
    {
        RuntimeConfiguration configuration = CreateConfiguration(ApplicationSafetyConfiguration.Empty);

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.True(decision.Allowed);
        Assert.Null(decision.MatchedAllowlistEntry);
        Assert.Contains("non-game utility", decision.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TitleOnlyTargetCanMatchTitleAllowlistEntry()
    {
        var safety = new ApplicationSafetyConfiguration(
            allowedApplications:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity(processName: null, windowTitleContains: "Target Canvas")),
            ]);
        RuntimeConfiguration configuration = CreateConfiguration(
            safety,
            RuntimeTargetSelector.ForWindowTitleContains("Target Canvas"));

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.True(decision.Allowed);
    }

    [Fact]
    public void NonMatchingGameTargetIsDenied()
    {
        var safety = new ApplicationSafetyConfiguration(
            allowedApplications:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("OtherApp")),
            ],
            gameProcessPatterns: ["Target*"]);
        RuntimeConfiguration configuration = CreateConfiguration(safety);

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configuration);

        Assert.False(decision.Allowed);
        Assert.Equal(ApplicationSafetyDenialReason.TargetNotAllowed, decision.DenialReason);
        Assert.Contains("not allowlisted", decision.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Target*", decision.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TargetGame.exe", "targetgame", true)]
    [InlineData("Game*Client", "GameOverlayClient", true)]
    [InlineData("*Client", "GameOverlayClient", true)]
    [InlineData("Target*", "OtherTarget", false)]
    public void GameProcessPatternsPreserveCurrentWildcardSemantics(
        string pattern,
        string processName,
        bool expectedGameCandidate)
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: [pattern]);

        ApplicationSafetyClassification classification =
            ApplicationSafetyPolicy.Classify(safety, new ApplicationIdentity(processName));

        Assert.Equal(expectedGameCandidate, classification.IsGameCandidate);
        Assert.Equal(expectedGameCandidate ? pattern : null, classification.MatchedGameCandidateRule);
    }

    [Fact]
    public void GameLibraryRootClassifiesExecutablePath()
    {
        string root = Path.Combine(Path.GetTempPath(), "mouse-shenanigans-games");
        var safety = new ApplicationSafetyConfiguration(gameLibraryRoots: [root]);
        RuntimeConfiguration configuration = CreateConfiguration(
            safety,
            RuntimeTargetSelector.Create("TargetApp", Path.Combine(root, "TargetApp.exe"), windowTitleContains: null));

        ApplicationSafetyClassification classification =
            ApplicationSafetyPolicy.Classify(safety, ApplicationIdentity.FromTargetSelector(configuration.TargetSelector));

        Assert.True(classification.IsGameCandidate);
        Assert.Equal(root, classification.MatchedGameCandidateRule);
    }

    [Fact]
    public void EmptyProtectedListDoesNotRequestExit()
    {
        ApplicationSelfExitDecision decision = ApplicationSafetyPolicy.EvaluateSelfExit(
            ApplicationSafetyConfiguration.Empty,
            [new ProcessSnapshot(123, new ApplicationIdentity("AnyApp"))]);

        Assert.False(decision.ShouldExit);
    }

    [Fact]
    public void ProtectedRunningApplicationRequestsExit()
    {
        var safety = new ApplicationSafetyConfiguration(
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("GameApp"), "Protected game"),
            ]);

        ApplicationSelfExitDecision decision = ApplicationSafetyPolicy.EvaluateSelfExit(
            safety,
            [new ProcessSnapshot(123, new ApplicationIdentity("GameApp"))]);

        Assert.True(decision.ShouldExit);
        Assert.Equal("Protected game", decision.MatchedProtectedDenyRule?.Label);
        Assert.Contains("Self-exit requested", decision.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NonAllowlistedGameCandidateRequestsExitOnlyWhenRuntimeEnabled()
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: ["GameApp"]);
        ProcessSnapshot process = new(123, new ApplicationIdentity("GameApp"));

        ApplicationSelfExitDecision enabledDecision = ApplicationSafetyPolicy.EvaluateSelfExit(
            safety,
            [process],
            runtimeEnabled: true);
        ApplicationSelfExitDecision disabledDecision = ApplicationSafetyPolicy.EvaluateSelfExit(
            safety,
            [process],
            runtimeEnabled: false);

        Assert.True(enabledDecision.ShouldExit);
        Assert.False(disabledDecision.ShouldExit);
    }

    [Fact]
    public void UnknownProcessIdentityFailsClosedWhenRuntimeEnabled()
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: ["GameApp"]);

        ApplicationSelfExitDecision decision = ApplicationSafetyPolicy.EvaluateSelfExit(
            safety,
            [new ProcessSnapshot(123, null)],
            runtimeEnabled: true);

        Assert.True(decision.ShouldExit);
        Assert.Contains("identity was unreadable", decision.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnknownProcessesAreNotTreatedAsDangerSignals()
    {
        var safety = new ApplicationSafetyConfiguration(
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("ConfiguredApp")),
            ]);

        ApplicationSelfExitDecision decision = ApplicationSafetyPolicy.EvaluateSelfExit(
            safety,
            [new ProcessSnapshot(123, new ApplicationIdentity("UnrelatedApp"))]);

        Assert.False(decision.ShouldExit);
    }

    private static RuntimeConfiguration CreateConfiguration(
        ApplicationSafetyConfiguration safety,
        RuntimeTargetSelector? targetSelector = null)
    {
        return RuntimeConfiguration.CreateFromConfiguredProfiles(
            targetSelector ?? RuntimeTargetSelector.ForProcessName("TargetApp.exe"),
            RuntimeProofOfConceptDefaults.ActiveProfileName,
            cursorLockEnabled: false,
            [RuntimeProofOfConceptDefaults.HorizontalInversionProfile],
            safety);
    }
}
