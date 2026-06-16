using MouseShenanigans.Tray;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayHotkeyControllerTests
{
    [Fact]
    public void DispatchHotkeyRunsResolvedRuntimeCommandAndRefreshesStatus()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var registrar = new RecordingHotkeyRegistrar
        {
            ResolvedCommands = { [42] = RuntimeCommand.ToggleRuntime },
        };
        var refreshRequests = 0;
        var controller = new TrayHotkeyController(
            registrar,
            new RuntimeCommandController(runtime),
            () => refreshRequests++);

        bool handled = controller.DispatchHotkey(42);

        Assert.True(handled);
        Assert.Equal(1, runtime.EnableRequests);
        Assert.Equal(1, refreshRequests);
        Assert.Equal(42, controller.LastReceivedHotkeyId);
        Assert.Equal(RuntimeCommand.ToggleRuntime, controller.LastDispatchedCommand);
    }

    [Fact]
    public void DispatchHotkeyIgnoresUnknownHotkeyIds()
    {
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var registrar = new RecordingHotkeyRegistrar();
        var refreshRequests = 0;
        var controller = new TrayHotkeyController(
            registrar,
            new RuntimeCommandController(runtime),
            () => refreshRequests++);

        bool handled = controller.DispatchHotkey(99);

        Assert.False(handled);
        Assert.Equal(0, runtime.EnableRequests);
        Assert.Equal(0, refreshRequests);
        Assert.Equal(99, controller.LastReceivedHotkeyId);
        Assert.Null(controller.LastDispatchedCommand);
    }

    [Fact]
    public void DispatchAllowedApplicationCaptureHotkeyRequestsConfirmationPrompt()
    {
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();
        var store = new RecordingConfigurationStore(configuration);
        var configurationController = new RuntimeConfigurationController(
            store,
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var targetReader = new StubTargetWindowReader(new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo("notepad", "Untitled - Notepad"),
            windowUnderCursor: null));
        var confirmationController = new ForegroundAllowlistConfirmationController(configurationController, targetReader);
        var runtime = new RecordingRuntimeController(RuntimeRemappingStatus.Disabled);
        var registrar = new RecordingHotkeyRegistrar
        {
            ResolvedCommands = { [77] = RuntimeCommand.CaptureForegroundAllowedApplication },
        };
        List<ForegroundAllowlistConfirmationRequest> promptRequests = [];
        var controller = new TrayHotkeyController(
            registrar,
            new RuntimeCommandController(
                runtime,
                configurationController,
                targetReader,
                foregroundAllowlistConfirmationController: confirmationController),
            () => { },
            promptRequests.Add);

        bool handled = controller.DispatchHotkey(77);

        Assert.True(handled);
        Assert.Single(promptRequests);
        Assert.Equal("notepad", promptRequests.Single().Identity.ProcessName);
        Assert.Empty(configurationController.Current.Safety.AllowedApplications);
        Assert.Empty(store.SavedConfigurations);
        Assert.Equal(0, runtime.EnableRequests);
        Assert.Equal(RuntimeCommand.CaptureForegroundAllowedApplication, controller.LastDispatchedCommand);
    }

    [Fact]
    public void RegisterStoresDegradedRegistrationStatus()
    {
        var registrar = new RecordingHotkeyRegistrar
        {
            RegistrationResult = HotkeyRegistrationResult.FromFailures(
            [
                new HotkeyRegistrationFailure(
                    new HotkeyBinding(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control, System.Windows.Forms.Keys.M),
                    1409,
                    "Hotkey is already registered."),
            ]),
        };
        var controller = new TrayHotkeyController(
            registrar,
            new RuntimeCommandController(new RecordingRuntimeController(RuntimeRemappingStatus.Disabled)),
            () => { });

        HotkeyRegistrationResult result = controller.Register(new IntPtr(123), []);

        Assert.False(result.Succeeded);
        Assert.Same(result, controller.RegistrationResult);
    }

    [Fact]
    public void HotkeyStatusTextReportsRegisteredAndDegradedStates()
    {
        Assert.Equal(
            "Hotkeys: registered - no hotkey received",
            TrayStatusFormatter.CreateHotkeyStatusText(HotkeyRegistrationResult.Success));

        Assert.Equal(
            "Hotkeys: registered - last ToggleRuntime (id 42)",
            TrayStatusFormatter.CreateHotkeyStatusText(
                HotkeyRegistrationResult.Success,
                RuntimeCommand.ToggleRuntime,
                42));

        HotkeyRegistrationResult degraded = HotkeyRegistrationResult.FromFailures(
        [
            new HotkeyRegistrationFailure(
                new HotkeyBinding(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control, System.Windows.Forms.Keys.M),
                1409,
                "Hotkey is already registered."),
        ]);

        Assert.Equal(
            "Hotkeys: degraded - ToggleRuntime Ctrl+M: Hotkey is already registered.",
            TrayStatusFormatter.CreateHotkeyStatusText(degraded));
    }

    private sealed class RecordingHotkeyRegistrar : IHotkeyRegistrar
    {
        public HotkeyRegistrationResult RegistrationResult { get; init; } = HotkeyRegistrationResult.Success;

        public Dictionary<int, RuntimeCommand> ResolvedCommands { get; } = [];

        public HotkeyRegistrationResult Register(IntPtr windowHandle, IReadOnlyCollection<HotkeyBinding> bindings)
        {
            return RegistrationResult;
        }

        public RuntimeCommand? TryResolveCommand(int hotkeyId)
        {
            return ResolvedCommands.TryGetValue(hotkeyId, out RuntimeCommand command)
                ? command
                : null;
        }

        public void UnregisterAll()
        {
        }

        public void Dispose()
        {
        }
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
        }

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
