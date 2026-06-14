using System.Text.Json;
using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class LocalControlEndpointHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void StatusResponseIncludesRuntimeProfileAndDegradedMessages()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        runtime.SetCursorLockEnabled(true);
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(runtime, configurationController),
            getDegradedStatusMessage: () => "Local control available at http://127.0.0.1:5178");

        LocalControlEndpointResult result = handler.GetStatus();

        Assert.Equal(200, result.StatusCode);
        var response = Assert.IsType<LocalControlRuntimeSnapshotResponse>(result.Body);
        Assert.True(response.Ok);
        Assert.Equal("disabled", response.State);
        Assert.True(response.CursorLockEnabled);
        Assert.Equal("TargetApp.exe", response.Target);
        Assert.Equal("horizontal-inversion", response.ActiveProfile);
        Assert.Equal(["horizontal-inversion", "double-right"], response.Profiles);
        Assert.Contains("Local control available", response.Message, StringComparison.Ordinal);

        string json = JsonSerializer.Serialize(response, JsonOptions);
        Assert.Contains("\"ok\":true", json, StringComparison.Ordinal);
        Assert.Contains("\"cursorLockEnabled\":true", json, StringComparison.Ordinal);
        Assert.Contains("\"target\":\"TargetApp.exe\"", json, StringComparison.Ordinal);
        Assert.Contains("\"activeProfile\":\"horizontal-inversion\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingProfileNameReturnsStableErrorShape()
    {
        LocalControlEndpointHandler handler = CreateHandler();

        LocalControlEndpointResult result = handler.SelectProfile(new LocalControlSelectProfileRequest(null));

        Assert.Equal(400, result.StatusCode);
        var response = Assert.IsType<LocalControlErrorResponse>(result.Body);
        Assert.False(response.Ok);
        Assert.Equal(LocalControlErrorCodes.MissingProfileName, response.Error);

        string json = JsonSerializer.Serialize(response, JsonOptions);
        Assert.Contains("\"ok\":false", json, StringComparison.Ordinal);
        Assert.Contains("\"error\":\"missing-profile-name\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownProfileReturnsProfileNotFoundWithoutChangingActiveProfile()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var handler = new LocalControlEndpointHandler(new RuntimeCommandController(runtime, configurationController));

        LocalControlEndpointResult result = handler.SelectProfile(new LocalControlSelectProfileRequest("missing"));

        Assert.Equal(400, result.StatusCode);
        var response = Assert.IsType<LocalControlErrorResponse>(result.Body);
        Assert.Equal(LocalControlErrorCodes.ProfileNotFound, response.Error);
        Assert.Equal("horizontal-inversion", configurationController.Current.ActiveProfileName);
        Assert.Empty(runtime.AppliedOptions);
        Assert.Empty(store.SavedConfigurations);
    }

    [Fact]
    public void RuntimeCommandEndpointsDispatchThroughSharedCommandBoundary()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var refreshRequests = 0;
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(runtime),
            requestStatusRefresh: () => refreshRequests++);

        handler.Execute(RuntimeCommand.EnableRuntime);
        handler.Execute(RuntimeCommand.ToggleRuntime);
        handler.Execute(RuntimeCommand.DisableRuntime);
        handler.Execute(RuntimeCommand.EmergencyDisable);

        Assert.Equal(1, runtime.EnableRequests);
        Assert.Equal(3, runtime.DisableRequests);
        Assert.Equal(4, refreshRequests);
    }

    [Fact]
    public void RuntimeCommandEndpointsRunThroughControlThreadDispatcher()
    {
        var commandRanInsideDispatcher = false;
        var isInsideDispatcher = false;
        var runtime = new RecordingRuntimeController(
            RuntimeRemappingStatus.Disabled,
            enableAction: () => commandRanInsideDispatcher = isInsideDispatcher);
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(runtime),
            runRequestOnControlThread: operation =>
            {
                isInsideDispatcher = true;
                try
                {
                    return operation();
                }
                finally
                {
                    isInsideDispatcher = false;
                }
            });

        handler.Execute(RuntimeCommand.EnableRuntime);

        Assert.True(commandRanInsideDispatcher);
    }

    [Fact]
    public void DisableAndEmergencyDisableReleaseCursorLockThroughRuntime()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Enabled);
        runtime.SetCursorLockEnabled(true);
        var handler = new LocalControlEndpointHandler(new RuntimeCommandController(runtime));

        handler.Execute(RuntimeCommand.DisableRuntime);
        runtime.SetCursorLockEnabled(true);
        handler.Execute(RuntimeCommand.EmergencyDisable);

        Assert.False(runtime.IsCursorLockEnabled);
        Assert.Equal(2, runtime.DisableRequests);
    }

    [Fact]
    public void CaptureForegroundTargetSuccessPersistsTargetAppliesOptionsAndRefreshesStatus()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var refreshRequests = 0;
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(
                runtime,
                configurationController,
                new StubTargetWindowReader(new TargetWindowSnapshot(
                    foregroundWindow: new TargetWindowInfo("notepad", "Untitled - Notepad"),
                    windowUnderCursor: null))),
            requestStatusRefresh: () => refreshRequests++);

        LocalControlEndpointResult result = handler.CaptureForegroundTarget();

        Assert.Equal(200, result.StatusCode);
        var response = Assert.IsType<LocalControlRuntimeSnapshotResponse>(result.Body);
        Assert.True(response.Ok);
        Assert.Equal("notepad.exe", response.Target);
        Assert.Equal("notepad", configurationController.Current.TargetSelector.ProcessName);
        Assert.Equal("notepad", store.SavedConfigurations.Single().TargetSelector.ProcessName);
        Assert.Equal("notepad", runtime.AppliedOptions.Single().TargetSelector.ProcessName);
        Assert.Equal(1, refreshRequests);
    }

    [Fact]
    public void CaptureForegroundTargetFailureReturnsErrorAndKeepsLastKnownGoodTarget()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var refreshRequests = 0;
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(
                runtime,
                configurationController,
                new StubTargetWindowReader(TargetWindowSnapshot.Empty)),
            requestStatusRefresh: () => refreshRequests++);

        LocalControlEndpointResult result = handler.CaptureForegroundTarget();

        Assert.Equal(400, result.StatusCode);
        var response = Assert.IsType<LocalControlErrorResponse>(result.Body);
        Assert.False(response.Ok);
        Assert.Equal(LocalControlErrorCodes.TargetCaptureFailed, response.Error);
        Assert.Contains("no foreground window", response.Message, StringComparison.Ordinal);
        Assert.Equal("TargetApp", configurationController.Current.TargetSelector.ProcessName);
        Assert.Empty(store.SavedConfigurations);
        Assert.Empty(runtime.AppliedOptions);
        Assert.Equal(1, refreshRequests);
    }

    [Fact]
    public void ProfilesEndpointReturnsLoadedProfileNamesAndActiveProfile()
    {
        RuntimeConfiguration configuration = CreateConfiguration().WithActiveProfile("double-right");
        LocalControlEndpointHandler handler = CreateHandler(configuration);

        LocalControlEndpointResult result = handler.GetProfiles();

        Assert.Equal(200, result.StatusCode);
        var response = Assert.IsType<LocalControlProfilesResponse>(result.Body);
        Assert.True(response.Ok);
        Assert.Equal("double-right", response.ActiveProfile);
        Assert.Equal(["horizontal-inversion", "double-right"], response.Profiles);
    }

    [Fact]
    public void SelectProfileSuccessPersistsSelectionAppliesOptionsAndRefreshesStatus()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var refreshRequests = 0;
        var handler = new LocalControlEndpointHandler(
            new RuntimeCommandController(runtime, configurationController),
            requestStatusRefresh: () => refreshRequests++);

        LocalControlEndpointResult result = handler.SelectProfile(new LocalControlSelectProfileRequest("double-right"));

        Assert.Equal(200, result.StatusCode);
        var response = Assert.IsType<LocalControlRuntimeSnapshotResponse>(result.Body);
        Assert.Equal("double-right", response.ActiveProfile);
        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
        Assert.Equal("double-right", store.SavedConfigurations.Single().ActiveProfileName);
        Assert.Equal(1, refreshRequests);
    }

    [Fact]
    public void ReloadConfigurationSuccessAppliesReloadedOptions()
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
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var handler = new LocalControlEndpointHandler(new RuntimeCommandController(runtime, configurationController));

        LocalControlEndpointResult result = handler.ReloadConfiguration();

        Assert.Equal(200, result.StatusCode);
        var response = Assert.IsType<LocalControlRuntimeSnapshotResponse>(result.Body);
        Assert.Equal("double-right", response.ActiveProfile);
        Assert.Equal("double-right", runtime.AppliedOptions.Single().ActiveProfile.Name);
    }

    [Fact]
    public void ReloadConfigurationFailureReturnsErrorAndKeepsLastKnownGood()
    {
        RuntimeConfiguration configuration = CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration)
        {
            ReloadException = new InvalidDataException("bad config"),
        };
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var handler = new LocalControlEndpointHandler(new RuntimeCommandController(runtime, configurationController));

        LocalControlEndpointResult result = handler.ReloadConfiguration();

        Assert.Equal(400, result.StatusCode);
        var response = Assert.IsType<LocalControlErrorResponse>(result.Body);
        Assert.Equal(LocalControlErrorCodes.ConfigurationReloadFailed, response.Error);
        Assert.Equal("horizontal-inversion", configurationController.Current.ActiveProfileName);
        Assert.Empty(runtime.AppliedOptions);
    }

    private static LocalControlEndpointHandler CreateHandler(RuntimeConfiguration? configuration = null)
    {
        var configurationController = new RuntimeConfigurationController(
            new RecordingConfigurationStore(configuration ?? CreateConfiguration()),
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        return new LocalControlEndpointHandler(new RuntimeCommandController(runtime, configurationController));
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

    private sealed class RecordingRuntimeController(
        RuntimeRemappingStatus status,
        Action? enableAction = null,
        Action? disableAction = null) : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; private set; } = status;

        public bool IsCursorLockEnabled { get; private set; }

        public int EnableRequests { get; private set; }

        public int DisableRequests { get; private set; }

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
            EnableRequests++;
            enableAction?.Invoke();
            Status = RuntimeRemappingStatus.Enabled;
        }

        public void Disable()
        {
            DisableRequests++;
            disableAction?.Invoke();
            IsCursorLockEnabled = false;
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
