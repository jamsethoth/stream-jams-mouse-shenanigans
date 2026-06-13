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
        Assert.Collection(
            configuration.ProfileNames,
            profileName => Assert.Equal("horizontal-inversion", profileName));
        Assert.Collection(
            configuration.ConfiguredProfiles,
            profile => Assert.Equal("horizontal-inversion", profile.Name));
    }

    [Fact]
    public void DeserializeRejectsEmptyProfileCollection()
    {
        const string json = """
            {
              "target": { "processName": "Streamer.bot.exe" },
              "activeProfile": "horizontal-inversion",
              "cursorLockEnabled": true,
              "profiles": []
            }
            """;

        Assert.Throws<InvalidDataException>(() => RuntimeConfigurationJsonSerializer.Deserialize(json));
    }

    [Fact]
    public void DeserializeConfigUsesConfiguredProfiles()
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
            profileName => Assert.Equal("double-right", profileName));
        Assert.Collection(
            configuration.ConfiguredProfiles,
            profileName => Assert.Equal("double-right", profileName.Name));
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
    public void SerializeWritesConfiguredProfiles()
    {
        RuntimeConfiguration configuration = RuntimeConfiguration.CreateFromConfiguredProfiles(
            RuntimeTargetSelector.ForProcessName("Streamer.bot.exe"),
            RuntimeProofOfConceptDefaults.ActiveProfileName,
            cursorLockEnabled: true,
            [RuntimeProofOfConceptDefaults.HorizontalInversionProfile]);

        string json = RuntimeConfigurationJsonSerializer.Serialize(configuration);

        Assert.Contains("\"name\": \"horizontal-inversion\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializePreservesConfiguredProfilesAfterDeserialize()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(MultipleProfilesJson);

        string json = RuntimeConfigurationJsonSerializer.Serialize(configuration);

        Assert.Contains("\"name\": \"horizontal-inversion\"", json, StringComparison.Ordinal);
        Assert.Contains("\"name\": \"double-right\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void WithTargetSelectorKeepsConfiguredProfiles()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationJsonSerializer.Deserialize(MultipleProfilesJson);

        RuntimeConfiguration updated = configuration.WithTargetSelector(RuntimeTargetSelector.ForProcessName("notepad"));

        Assert.Equal("notepad", updated.TargetSelector.ProcessName);
        Assert.Equal(configuration.ActiveProfileName, updated.ActiveProfileName);
        Assert.Equal(configuration.CursorLockEnabled, updated.CursorLockEnabled);
        Assert.Equal(configuration.ProfileNames, updated.ProfileNames);
        Assert.Equal(
            configuration.ConfiguredProfiles.Select(profile => profile.Name),
            updated.ConfiguredProfiles.Select(profile => profile.Name));
    }

    [Fact]
    public void FallbackConfigurationMatchesProofOfConceptDefaults()
    {
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();

        Assert.Equal("Streamer.bot", configuration.TargetSelector.ProcessName);
        Assert.Equal(RuntimeProofOfConceptDefaults.ActiveProfileName, configuration.ActiveProfileName);
        Assert.True(configuration.CursorLockEnabled);
        Assert.Equal(RuntimeProofOfConceptDefaults.HorizontalInversionProfile, configuration.ActiveProfile);
        Assert.Collection(
            configuration.ProfileNames,
            profileName => Assert.Equal("horizontal-inversion", profileName));
        Assert.Collection(
            configuration.ConfiguredProfiles,
            profile => Assert.Equal("horizontal-inversion", profile.Name));
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

    private const string MultipleProfilesJson = """
        {
          "target": {
            "processName": "Streamer.bot.exe",
            "windowTitleContains": null
          },
          "activeProfile": "double-right",
          "cursorLockEnabled": true,
          "profiles": [
            {
              "name": "horizontal-inversion",
              "left": { "x": 1, "y": 0 },
              "right": { "x": -1, "y": 0 },
              "up": { "x": 0, "y": -1 },
              "down": { "x": 0, "y": 1 }
            },
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
}
