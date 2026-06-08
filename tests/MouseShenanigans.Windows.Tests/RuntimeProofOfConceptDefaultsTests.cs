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
        Assert.Equal(1.0, options.AbsoluteCorrectionScale);
    }
}
