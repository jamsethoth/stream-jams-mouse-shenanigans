using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeCommandControllerTests
{
    [Fact]
    public void ToggleEnablesDisabledRuntime()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime);

        controller.Toggle();

        Assert.Equal(1, runtime.EnableRequests);
        Assert.Equal(0, runtime.DisableRequests);
    }

    [Fact]
    public void ToggleDisablesEnabledRuntime()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var controller = new RuntimeCommandController(runtime);

        controller.Toggle();

        Assert.Equal(0, runtime.EnableRequests);
        Assert.Equal(1, runtime.DisableRequests);
    }

    [Fact]
    public void EmergencyDisableUsesRuntimeDisablePathWhenEnabled()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var controller = new RuntimeCommandController(runtime);

        controller.EmergencyDisable();

        Assert.Equal(1, runtime.DisableRequests);
    }

    [Fact]
    public void EmergencyDisableUsesRuntimeDisablePathWhenAlreadyDisabled()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime);

        controller.EmergencyDisable();

        Assert.Equal(1, runtime.DisableRequests);
    }

    [Fact]
    public void SelectProfilePersistsSelectionAndAppliesRuntimeOptions()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime, configurationController);

        RuntimeConfigurationOperationResult result = controller.SelectProfile("double-right");

        Assert.True(result.Succeeded);
        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
        Assert.Single(store.SavedConfigurations);
    }

    [Fact]
    public void ReloadConfigurationAppliesReloadedRuntimeOptions()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        RuntimeConfiguration reloaded = configuration.WithActiveProfile("double-right");
        var store = new RecordingConfigurationStore(configuration)
        {
            ReloadConfiguration = reloaded,
        };
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime, configurationController);

        RuntimeConfigurationOperationResult result = controller.ReloadConfiguration();

        Assert.True(result.Succeeded);
        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
    }

    [Fact]
    public void ReloadConfigurationFailureKeepsLastKnownGoodRuntimeOptions()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration)
        {
            ReloadException = new InvalidDataException("invalid"),
        };
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var controller = new RuntimeCommandController(runtime, configurationController);

        RuntimeConfigurationOperationResult result = controller.ReloadConfiguration();

        Assert.False(result.Succeeded);
        Assert.Empty(runtime.AppliedOptions);
        Assert.Same(configuration, configurationController.Current);
    }

    [Fact]
    public void CaptureForegroundTargetPersistsForegroundProcessAndAppliesRuntimeOptions()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var targetReader = new StubTargetWindowReader(new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo("notepad", "Untitled - Notepad"),
            windowUnderCursor: null));
        var controller = new RuntimeCommandController(runtime, configurationController, targetReader);

        RuntimeConfigurationOperationResult result = controller.CaptureForegroundTarget();

        Assert.True(result.Succeeded);
        Assert.Equal("notepad", configurationController.Current.TargetSelector.ProcessName);
        Assert.Single(store.SavedConfigurations);
        Assert.Equal("notepad", store.SavedConfigurations[0].TargetSelector.ProcessName);
        Assert.Equal("notepad", runtime.AppliedOptions.Single().TargetSelector.ProcessName);
    }

    [Fact]
    public void CaptureForegroundTargetFallsBackToWindowTitleWhenProcessNameIsUnavailable()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var targetReader = new StubTargetWindowReader(new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo(processName: null, title: "Untitled - Notepad"),
            windowUnderCursor: null));
        var controller = new RuntimeCommandController(runtime, configurationController, targetReader);

        RuntimeConfigurationOperationResult result = controller.CaptureForegroundTarget();

        Assert.True(result.Succeeded);
        Assert.Equal("Untitled - Notepad", configurationController.Current.TargetSelector.WindowTitleContains);
        Assert.Null(configurationController.Current.TargetSelector.ProcessName);
        Assert.Equal("Untitled - Notepad", runtime.AppliedOptions.Single().TargetSelector.WindowTitleContains);
    }

    [Fact]
    public void CaptureForegroundTargetFailureKeepsLastKnownGoodRuntimeOptions()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        var controller = new RuntimeCommandController(
            runtime,
            configurationController,
            new StubTargetWindowReader(TargetWindowSnapshot.Empty));

        RuntimeConfigurationOperationResult result = controller.CaptureForegroundTarget();

        Assert.False(result.Succeeded);
        Assert.Same(configuration, configurationController.Current);
        Assert.Empty(store.SavedConfigurations);
        Assert.Empty(runtime.AppliedOptions);
        Assert.Contains("Target capture failed", configurationController.StatusMessage, StringComparison.Ordinal);
    }

    private sealed class RecordingRuntimeController(RuntimeRemappingStatus status) : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; private set; } = status;

        public bool IsCursorLockEnabled { get; private set; }

        public int EnableRequests { get; private set; }

        public int DisableRequests { get; private set; }

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void ApplyOptions(RuntimeRemappingOptions options)
        {
            AppliedOptions.Add(options);
        }

        public List<RuntimeRemappingOptions> AppliedOptions { get; } = [];

        public void Enable()
        {
            EnableRequests++;
            Status = RuntimeRemappingStatus.Enabled;
        }

        public void Disable()
        {
            DisableRequests++;
            Status = RuntimeRemappingStatus.Disabled;
        }

        public void Dispose()
        {
        }
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

    private sealed class StubTargetWindowReader(TargetWindowSnapshot snapshot) : ITargetWindowReader
    {
        public TargetWindowSnapshot ReadSnapshot()
        {
            return snapshot;
        }
    }
}
