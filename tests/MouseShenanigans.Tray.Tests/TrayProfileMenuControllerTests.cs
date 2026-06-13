using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayProfileMenuControllerTests
{
    [Fact]
    public void RefreshProfilesListsProfilesAndMarksActiveProfile()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController();
        var commandController = new RuntimeCommandController(runtime, configurationController);
        var profileMenuItem = new ToolStripMenuItem("Profiles");
        var controller = new TrayProfileMenuController(profileMenuItem, commandController, () => { });

        controller.RefreshProfiles();

        Assert.Equal(2, profileMenuItem.DropDownItems.Count);
        Assert.Equal("horizontal-inversion", profileMenuItem.DropDownItems[0].Text);
        Assert.True(((ToolStripMenuItem)profileMenuItem.DropDownItems[0]).Checked);
        Assert.Equal("double-right", profileMenuItem.DropDownItems[1].Text);
        Assert.False(((ToolStripMenuItem)profileMenuItem.DropDownItems[1]).Checked);
    }

    [Fact]
    public void SelectingProfileAppliesRuntimeOptionsPersistsSelectionAndRefreshesStatus()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController();
        var commandController = new RuntimeCommandController(runtime, configurationController);
        var profileMenuItem = new ToolStripMenuItem("Profiles");
        var refreshRequests = 0;
        var controller = new TrayProfileMenuController(profileMenuItem, commandController, () => refreshRequests++);
        controller.RefreshProfiles();

        ((ToolStripMenuItem)profileMenuItem.DropDownItems[1]).PerformClick();

        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
        Assert.Equal("double-right", store.SavedConfigurations.Single().ActiveProfileName);
        Assert.Equal(1, refreshRequests);
        Assert.True(((ToolStripMenuItem)profileMenuItem.DropDownItems[1]).Checked);
    }

    [Fact]
    public void ReloadConfigurationAppliesReloadedProfileListAndRefreshesStatus()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        RuntimeConfiguration reloaded = configuration.WithActiveProfile("double-right");
        var store = new RecordingConfigurationStore(configuration)
        {
            ReloadConfiguration = reloaded,
        };
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController();
        var commandController = new RuntimeCommandController(runtime, configurationController);
        var profileMenuItem = new ToolStripMenuItem("Profiles");
        var refreshRequests = 0;
        var controller = new TrayProfileMenuController(profileMenuItem, commandController, () => refreshRequests++);

        controller.ReloadConfiguration();

        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
        Assert.Equal(1, refreshRequests);
        Assert.True(((ToolStripMenuItem)profileMenuItem.DropDownItems[1]).Checked);
    }

    [Fact]
    public void ReloadFailureKeepsLastKnownGoodConfigurationAndReportsStatus()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration)
        {
            ReloadException = new InvalidDataException("bad config"),
        };
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController();
        var commandController = new RuntimeCommandController(runtime, configurationController);
        var profileMenuItem = new ToolStripMenuItem("Profiles");
        var refreshRequests = 0;
        var controller = new TrayProfileMenuController(profileMenuItem, commandController, () => refreshRequests++);

        controller.ReloadConfiguration();

        Assert.Empty(runtime.AppliedOptions);
        Assert.Equal("horizontal-inversion", configurationController.Current.ActiveProfileName);
        Assert.Contains("reload failed", configurationController.StatusMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, refreshRequests);
        Assert.True(((ToolStripMenuItem)profileMenuItem.DropDownItems[0]).Checked);
    }

    private sealed class RecordingRuntimeController : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; private set; } = RuntimeRemappingStatus.Disabled;

        public bool IsCursorLockEnabled { get; private set; }

        public List<RuntimeRemappingOptions> AppliedOptions { get; } = [];

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void ApplyOptions(RuntimeRemappingOptions options)
        {
            AppliedOptions.Add(options);
            IsCursorLockEnabled = options.CursorLockEnabled;
        }

        public void Enable()
        {
            Status = RuntimeRemappingStatus.Enabled;
        }

        public void Disable()
        {
            Status = RuntimeRemappingStatus.Disabled;
        }

        public void Dispose()
        {
        }
    }

    private static RuntimeConfiguration CreateConfiguration()
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
            SavedConfigurations.Add(configuration);
        }
    }
}
