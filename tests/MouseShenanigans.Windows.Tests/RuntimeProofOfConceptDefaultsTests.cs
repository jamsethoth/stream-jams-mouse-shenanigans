using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeProofOfConceptDefaultsTests
{
    [Fact]
    public void CreateOptionsTargetsStreamerBotExecutableName()
    {
        RuntimeRemappingOptions options = RuntimeProofOfConceptDefaults.CreateOptions();
        var snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: "Streamer.bot", title: "Streamer.bot"),
            windowUnderCursor: null);

        Assert.Equal("Streamer.bot.exe", RuntimeProofOfConceptDefaults.TargetProcessName);
        Assert.True(options.TargetSelector.IsMatch(snapshot));
        Assert.True(options.CursorLockEnabled);
        Assert.Equal(1.0, options.AbsoluteCorrectionScale);
    }

    [Fact]
    public void SerializedFallbackConfigurationKeepsHorizontalInversionBehavior()
    {
        string json = RuntimeConfigurationJsonSerializer.Serialize(RuntimeProofOfConceptDefaults.CreateConfiguration());
        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(json);
        var engine = new AbsoluteCursorRemappingDecisionEngine(configuration.ActiveProfile);

        Assert.True(configuration.CursorLockEnabled);

        AbsoluteCursorRemappingDecision decision = engine.Decide(
            new RuntimeMouseMovement(dx: 5, dy: 0),
            isEnabled: true,
            targetMatches: true,
            anchorPosition: new ScreenPoint(100, 50));

        Assert.Equal(new ScreenPoint(95, 50), decision.TargetPosition);
    }
}
