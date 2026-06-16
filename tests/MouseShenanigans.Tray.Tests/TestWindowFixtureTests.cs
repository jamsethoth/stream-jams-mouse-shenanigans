using MouseShenanigans.TestWindowFixture;

namespace MouseShenanigans.Tray.Tests;

public sealed class TestWindowFixtureTests
{
    [Fact]
    public void FixtureOptionsParseStableTitleAndReadinessPath()
    {
        TestWindowFixtureOptions options = TestWindowFixtureOptions.Parse(
            ["--title", "Stable Fixture", "--ready-file", @"C:\Temp\fixture.ready"]);

        Assert.Equal("Stable Fixture", options.WindowTitle);
        Assert.Equal(@"C:\Temp\fixture.ready", options.ReadyFilePath);
    }

    [Fact]
    public void FixtureReadinessSignalWritesUtf8StatusFile()
    {
        string readyFilePath = Path.Combine(
            Path.GetTempPath(),
            "MouseShenanigans.Tests",
            Guid.NewGuid().ToString("N"),
            "fixture.ready");

        TestWindowFixtureReadinessSignal.Write(readyFilePath, TestWindowFixtureOptions.DefaultWindowTitle);

        string text = File.ReadAllText(readyFilePath);
        Assert.Contains("ready=true", text, StringComparison.Ordinal);
        Assert.Contains(TestWindowFixtureOptions.DefaultWindowTitle, text, StringComparison.Ordinal);
        Assert.Contains("processName=", text, StringComparison.Ordinal);
    }
}
