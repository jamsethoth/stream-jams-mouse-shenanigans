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
    public void DeserializeConfigWithNoCustomProfilesKeepsBuiltInProfileAvailable()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "horizontal-inversion",
              "cursorLockEnabled": true,
              "profiles": []
            }
            """;

        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(json);

        Assert.Equal("horizontal-inversion", configuration.ActiveProfileName);
        Assert.Equal(BuiltInRemappingProfiles.HorizontalInversion.Name, configuration.ActiveProfile.Name);
        Assert.Contains(BuiltInRemappingProfiles.HorizontalInversion.Name, configuration.ProfileNames);
    }

    [Fact]
    public void DeserializeConfigAddsCustomProfilesToBuiltInProfiles()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "double-right",
              "cursorLockEnabled": true,
              "profiles": [
                {
                  "name": "double-right",
                  "left": { "x": -1, "y": 0 },
                  "right": { "x": 2, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(json);

        Assert.Equal("double-right", configuration.ActiveProfileName);
        Assert.Collection(
            configuration.ProfileNames,
            profileName => Assert.Equal("horizontal-inversion", profileName),
            profileName => Assert.Equal("double-right", profileName));
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
    public void DeserializeRejectsMissingCustomActiveProfile()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "missing",
              "cursorLockEnabled": false,
              "profiles": []
            }
            """;

        Assert.Throws<InvalidDataException>(() => RuntimeConfigurationJsonSerializer.Deserialize(json));
    }

    [Fact]
    public void SerializeOmitsBuiltInProfiles()
    {
        string json = RuntimeConfigurationJsonSerializer.Serialize(RuntimeProofOfConceptDefaults.CreateConfiguration());

        Assert.Contains("\"profiles\": []", json, StringComparison.Ordinal);
    }

    [Fact]
    public void FallbackConfigurationMatchesProofOfConceptDefaults()
    {
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();

        Assert.Equal("Streamer.bot", configuration.TargetSelector.ProcessName);
        Assert.Equal(BuiltInRemappingProfiles.HorizontalInversion.Name, configuration.ActiveProfileName);
        Assert.True(configuration.CursorLockEnabled);
        Assert.Same(BuiltInRemappingProfiles.HorizontalInversion, configuration.ActiveProfile);
    }

    [Fact]
    public void WithTargetSelectorKeepsProfilesAndCursorLock()
    {
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();

        RuntimeConfiguration updated = configuration.WithTargetSelector(RuntimeTargetSelector.ForProcessName("notepad"));

        Assert.Equal("notepad", updated.TargetSelector.ProcessName);
        Assert.Equal(configuration.ActiveProfileName, updated.ActiveProfileName);
        Assert.Equal(configuration.CursorLockEnabled, updated.CursorLockEnabled);
        Assert.Equal(configuration.ProfileNames, updated.ProfileNames);
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
