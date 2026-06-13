using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeConfigurationControllerTests
{
    [Fact]
    public void SelectProfilePersistsSelection()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.SelectProfile("double-right");

        Assert.True(result.Succeeded);
        Assert.Equal("double-right", controller.Current.ActiveProfileName);
        Assert.Single(store.SavedConfigurations);
        Assert.Equal("double-right", store.SavedConfigurations[0].ActiveProfileName);
        Assert.Equal(
            configuration.ConfiguredProfiles.Select(profile => profile.Name),
            store.SavedConfigurations[0].ConfiguredProfiles.Select(profile => profile.Name));
    }

    [Fact]
    public void SelectProfileKeepsRuntimeSelectionWhenSaveFails()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration)
        {
            SaveException = new UnauthorizedAccessException("denied"),
        };
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.SelectProfile("double-right");

        Assert.False(result.Succeeded);
        Assert.Equal("double-right", controller.Current.ActiveProfileName);
        Assert.Contains("save failed", controller.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectTargetPersistsTargetSelection()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.SelectTarget(RuntimeTargetSelector.ForProcessName("notepad"));

        Assert.True(result.Succeeded);
        Assert.Equal("notepad", controller.Current.TargetSelector.ProcessName);
        Assert.Single(store.SavedConfigurations);
        Assert.Equal("notepad", store.SavedConfigurations[0].TargetSelector.ProcessName);
        Assert.Equal(
            configuration.ConfiguredProfiles.Select(profile => profile.Name),
            store.SavedConfigurations[0].ConfiguredProfiles.Select(profile => profile.Name));
    }

    [Fact]
    public void ReportOperationFailurePreservesCurrentConfiguration()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.ReportOperationFailure("Target capture failed.");

        Assert.False(result.Succeeded);
        Assert.Same(configuration, controller.Current);
        Assert.Equal("Target capture failed.", controller.StatusMessage);
        Assert.Empty(store.SavedConfigurations);
    }

    [Fact]
    public void ReloadSuccessReplacesCurrentConfiguration()
    {
        RuntimeConfiguration initial = CreateConfiguration();
        RuntimeConfiguration reloaded = initial.WithActiveProfile("double-right");
        var store = new RecordingConfigurationStore(initial)
        {
            ReloadConfiguration = reloaded,
        };
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.Reload();

        Assert.True(result.Succeeded);
        Assert.Same(reloaded, controller.Current);
    }

    [Fact]
    public void ReloadFailureKeepsLastKnownGoodConfiguration()
    {
        RuntimeConfiguration initial = CreateConfiguration();
        var store = new RecordingConfigurationStore(initial)
        {
            ReloadException = new InvalidDataException("invalid"),
        };
        var controller = new RuntimeConfigurationController(store, RuntimeProofOfConceptDefaults.CreateConfiguration());

        RuntimeConfigurationOperationResult result = controller.Reload();

        Assert.False(result.Succeeded);
        Assert.Same(initial, controller.Current);
        Assert.Contains("reload failed", controller.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    public static RuntimeConfiguration CreateConfiguration()
    {
        RemappingProfile doubleRight = new(
            "double-right",
            left: new MovementVector(-1, 0),
            right: new MovementVector(2, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));

        return RuntimeConfiguration.Create(
            RuntimeTargetSelector.ForProcessName("TargetApp.exe"),
            RuntimeProofOfConceptDefaults.ActiveProfileName,
            cursorLockEnabled: false,
            RemappingProfileSet.Create([RuntimeProofOfConceptDefaults.HorizontalInversionProfile, doubleRight]));
    }

    private sealed class RecordingConfigurationStore(RuntimeConfiguration initialConfiguration) : IRuntimeConfigurationStore
    {
        public string ConfigurationPath => "config.json";

        public RuntimeConfiguration? ReloadConfiguration { get; init; }

        public Exception? ReloadException { get; init; }

        public Exception? SaveException { get; init; }

        public List<RuntimeConfiguration> SavedConfigurations { get; } = [];

        public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
        {
            return new RuntimeConfigurationLoadResult(initialConfiguration, UsedFallback: false, ErrorMessage: null);
        }

        public RuntimeConfiguration LoadRequired()
        {
            if (ReloadException is not null)
            {
                throw ReloadException;
            }

            return ReloadConfiguration ?? initialConfiguration;
        }

        public void Save(RuntimeConfiguration configuration)
        {
            if (SaveException is not null)
            {
                throw SaveException;
            }

            SavedConfigurations.Add(configuration);
        }
    }
}
