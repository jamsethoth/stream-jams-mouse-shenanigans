using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class ForegroundAllowlistConfirmationControllerTests
{
    [Fact]
    public void RequestForegroundConfirmationCreatesPendingRecordWithoutPersisting()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var confirmationController = new ForegroundAllowlistConfirmationController(
            configurationController,
            new StubTargetWindowReader(new TargetWindowSnapshot(
                foregroundWindow: new TargetWindowInfo("notepad", "Untitled - Notepad"),
                windowUnderCursor: null)));

        ForegroundAllowlistConfirmationRequestResult result =
            confirmationController.RequestForegroundConfirmation(ForegroundAllowlistConfirmationSource.Hotkey);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Request);
        Assert.Equal(ForegroundAllowlistConfirmationStatus.Pending, result.Request.Status);
        Assert.Equal("notepad", result.Request.Identity.ProcessName);
        Assert.Empty(store.SavedConfigurations);
        Assert.Empty(configurationController.Current.Safety.AllowedApplications);
    }

    [Fact]
    public void ConfirmPersistsCapturedApplication()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var confirmationController = CreateController(configurationController, "notepad");
        ForegroundAllowlistConfirmationRequest request =
            confirmationController.RequestForegroundConfirmation(ForegroundAllowlistConfirmationSource.LocalControl).Request!;

        ForegroundAllowlistConfirmationCompletionResult result = confirmationController.Confirm(request.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(ForegroundAllowlistConfirmationStatus.Accepted, result.Request?.Status);
        Assert.Single(configurationController.Current.Safety.AllowedApplications);
        Assert.Equal("notepad", store.SavedConfigurations.Single().Safety.AllowedApplications.Single().Identity.ProcessName);
    }

    [Fact]
    public void CancelLeavesAllowlistUnchanged()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var confirmationController = CreateController(configurationController, "notepad");
        ForegroundAllowlistConfirmationRequest request =
            confirmationController.RequestForegroundConfirmation(ForegroundAllowlistConfirmationSource.Hotkey).Request!;

        ForegroundAllowlistConfirmationCompletionResult result = confirmationController.Cancel(request.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(ForegroundAllowlistConfirmationStatus.Canceled, result.Request?.Status);
        Assert.Empty(configurationController.Current.Safety.AllowedApplications);
        Assert.Empty(store.SavedConfigurations);
    }

    [Fact]
    public void ConfirmExistingAllowlistEntryDoesNotCreateDuplicate()
    {
        var safety = new ApplicationSafetyConfiguration(
            allowedApplications:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("notepad")),
            ]);
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration().WithSafety(safety);
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var confirmationController = CreateController(configurationController, "notepad");
        ForegroundAllowlistConfirmationRequest request =
            confirmationController.RequestForegroundConfirmation(ForegroundAllowlistConfirmationSource.Hotkey).Request!;

        ForegroundAllowlistConfirmationCompletionResult result = confirmationController.Confirm(request.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(ForegroundAllowlistConfirmationStatus.AlreadyAllowed, result.Request?.Status);
        Assert.Single(configurationController.Current.Safety.AllowedApplications);
        Assert.Empty(store.SavedConfigurations);
    }

    [Fact]
    public void CaptureFailsWhenForegroundIdentityIsUnavailable()
    {
        RuntimeConfiguration configuration = RuntimeConfigurationControllerTests.CreateConfiguration();
        var configurationController = new RuntimeConfigurationController(
            new RecordingConfigurationStore(configuration),
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var confirmationController = new ForegroundAllowlistConfirmationController(
            configurationController,
            new StubTargetWindowReader(TargetWindowSnapshot.Empty));

        ForegroundAllowlistConfirmationRequestResult result =
            confirmationController.RequestForegroundConfirmation(ForegroundAllowlistConfirmationSource.Hotkey);

        Assert.False(result.Succeeded);
        Assert.Null(result.Request);
        Assert.Contains("no usable", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ForegroundAllowlistConfirmationController CreateController(
        RuntimeConfigurationController configurationController,
        string processName)
    {
        return new ForegroundAllowlistConfirmationController(
            configurationController,
            new StubTargetWindowReader(new TargetWindowSnapshot(
                foregroundWindow: new TargetWindowInfo(processName, $"{processName} title"),
                windowUnderCursor: null)));
    }

    private sealed class RecordingConfigurationStore(RuntimeConfiguration initialConfiguration) : IRuntimeConfigurationStore
    {
        public string ConfigurationPath => "config.json";

        public List<RuntimeConfiguration> SavedConfigurations { get; } = [];

        public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
        {
            return new RuntimeConfigurationLoadResult(initialConfiguration, UsedFallback: false, ErrorMessage: null);
        }

        public RuntimeConfiguration LoadRequired()
        {
            return initialConfiguration;
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
