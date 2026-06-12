using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeConfigurationTests
{
    [Fact]
    public void DeserializeValidConfigCreatesRuntimeConfiguration()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(ValidJson);

        Assert.Equal("Streamer.bot", configuration.TargetSelector.ProcessName);
        Assert.Equal("horizontal-inversion", configuration.ActiveProfileName);
        Assert.True(configuration.CursorLockEnabled);
        Assert.Equal("horizontal-inversion", configuration.ActiveProfile.Name);
    }

    [Fact]
    public void DeserializeRejectsMissingTarget()
    {
        const string json = """
            {
              "activeProfile": "horizontal-inversion",
              "cursorLockEnabled": false,
              "profiles": [
                {
                  "name": "horizontal-inversion",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        Assert.Throws<InvalidDataException>(() => RuntimeConfigurationJsonSerializer.Deserialize(json));
    }

    [Fact]
    public void DeserializeRejectsMissingActiveProfile()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "missing",
              "cursorLockEnabled": false,
              "profiles": [
                {
                  "name": "horizontal-inversion",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        Assert.Throws<InvalidDataException>(() => RuntimeConfigurationJsonSerializer.Deserialize(json));
    }

    [Fact]
    public void DeserializeRejectsInvalidProfileCollection()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "horizontal-inversion",
              "cursorLockEnabled": false,
              "profiles": []
            }
            """;

        Assert.Throws<InvalidDataException>(() => RuntimeConfigurationJsonSerializer.Deserialize(json));
    }

    [Fact]
    public void FallbackConfigurationMatchesProofOfConceptDefaults()
    {
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();

        Assert.Equal("Streamer.bot", configuration.TargetSelector.ProcessName);
        Assert.Equal(BuiltInRemappingProfiles.HorizontalInversion.Name, configuration.ActiveProfileName);
        Assert.False(configuration.CursorLockEnabled);
        Assert.Same(BuiltInRemappingProfiles.HorizontalInversion, configuration.ActiveProfile);
    }

    private const string ValidJson = """
        {
          "target": {
            "processName": "Streamer.bot.exe",
            "windowTitleContains": null
          },
          "activeProfile": "horizontal-inversion",
          "cursorLockEnabled": true,
          "profiles": [
            {
              "name": "horizontal-inversion",
              "left": { "x": 1, "y": 0 },
              "right": { "x": -1, "y": 0 },
              "up": { "x": 0, "y": -1 },
              "down": { "x": 0, "y": 1 }
            }
          ]
        }
        """;
}
