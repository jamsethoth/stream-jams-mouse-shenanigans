namespace MouseShenanigans.Windows;

public sealed class RuntimeCommandController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly RuntimeConfigurationController? configurationController;
    private readonly ITargetWindowReader? targetWindowReader;
    private readonly bool enableApplicationSafety;
    private readonly ForegroundAllowlistConfirmationController? foregroundAllowlistConfirmationController;
    private readonly IDiagnosticRecorder diagnosticRecorder;
    private string? applicationSafetyStatusMessage;

    public RuntimeCommandController(
        IRuntimeRemappingController runtime,
        RuntimeConfigurationController? configurationController = null,
        ITargetWindowReader? targetWindowReader = null,
        bool enableApplicationSafety = false,
        ForegroundAllowlistConfirmationController? foregroundAllowlistConfirmationController = null,
        IDiagnosticRecorder? diagnosticRecorder = null)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.configurationController = configurationController;
        this.targetWindowReader = targetWindowReader;
        this.enableApplicationSafety = enableApplicationSafety;
        this.foregroundAllowlistConfirmationController = foregroundAllowlistConfirmationController;
        this.diagnosticRecorder = diagnosticRecorder ?? NullDiagnosticRecorder.Instance;
    }

    public RuntimeConfiguration? CurrentConfiguration => configurationController?.Current;

    public string? ConfigurationStatusMessage => configurationController?.StatusMessage;

    public string? ApplicationSafetyStatusMessage
    {
        get
        {
            string?[] messages =
            [
                applicationSafetyStatusMessage,
                foregroundAllowlistConfirmationController?.StatusMessage,
            ];

            string[] nonEmptyMessages = messages
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Select(message => message!)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return nonEmptyMessages.Length == 0
                ? null
                : string.Join("; ", nonEmptyMessages);
        }
    }

    public RuntimeRemappingStatus RuntimeStatus => runtime.Status;

    public bool IsCursorLockEnabled => runtime.IsCursorLockEnabled;

    public void Execute(RuntimeCommand command)
    {
        switch (command)
        {
            case RuntimeCommand.EnableRuntime:
                Enable();
                break;
            case RuntimeCommand.DisableRuntime:
                Disable();
                break;
            case RuntimeCommand.ToggleRuntime:
                Toggle();
                break;
            case RuntimeCommand.EmergencyDisable:
                EmergencyDisable();
                break;
            case RuntimeCommand.CaptureForegroundTarget:
                CaptureForegroundTarget();
                break;
            case RuntimeCommand.CaptureForegroundAllowedApplication:
                CaptureForegroundAllowedApplication(ForegroundAllowlistConfirmationSource.Hotkey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown runtime command.");
        }
    }

    public void Enable()
    {
        if (!AllowEnable())
        {
            return;
        }

        runtime.Enable();
    }

    public void Disable()
    {
        runtime.Disable();
    }

    public void Toggle()
    {
        if (runtime.Status.State == RuntimeRemappingState.Enabled)
        {
            runtime.Disable();
            return;
        }

        Enable();
    }

    public void EmergencyDisable()
    {
        runtime.Disable();
    }

    public RuntimeConfigurationOperationResult SelectProfile(string profileName)
    {
        RuntimeConfigurationController controller = GetConfigurationController();
        RuntimeConfigurationOperationResult result = controller.SelectProfile(profileName);
        runtime.ApplyOptions(result.Configuration.CreateRuntimeOptions());
        return result;
    }

    public RuntimeConfigurationOperationResult ReloadConfiguration()
    {
        RuntimeConfigurationController controller = GetConfigurationController();
        RuntimeConfigurationOperationResult result = controller.Reload();
        if (result.Succeeded)
        {
            runtime.ApplyOptions(result.Configuration.CreateRuntimeOptions());
        }

        return result;
    }

    public RuntimeConfigurationOperationResult CaptureForegroundTarget()
    {
        RuntimeConfigurationController controller = GetConfigurationController();
        ITargetWindowReader reader = GetTargetWindowReader();
        TargetWindowInfo? foregroundWindow = reader.ReadSnapshot().ForegroundWindow;

        if (foregroundWindow is null)
        {
            return controller.ReportOperationFailure("Target capture failed: no foreground window was available.");
        }

        RuntimeTargetSelector? targetSelector = CreateTargetSelector(foregroundWindow);
        if (targetSelector is null)
        {
            return controller.ReportOperationFailure("Target capture failed: foreground window identity was unavailable.");
        }

        RuntimeConfigurationOperationResult result = controller.SelectTarget(targetSelector);
        runtime.ApplyOptions(result.Configuration.CreateRuntimeOptions());
        return result;
    }

    public ForegroundAllowlistConfirmationRequestResult CaptureForegroundAllowedApplication(
        ForegroundAllowlistConfirmationSource source)
    {
        ForegroundAllowlistConfirmationController controller = GetForegroundAllowlistConfirmationController();
        ForegroundAllowlistConfirmationRequestResult result = controller.RequestForegroundConfirmation(source);
        applicationSafetyStatusMessage = result.Message;
        return result;
    }

    private RuntimeConfigurationController GetConfigurationController()
    {
        return configurationController
            ?? throw new InvalidOperationException("Runtime configuration is not available.");
    }

    private ITargetWindowReader GetTargetWindowReader()
    {
        return targetWindowReader
            ?? throw new InvalidOperationException("Target window reading is not available.");
    }

    private ForegroundAllowlistConfirmationController GetForegroundAllowlistConfirmationController()
    {
        return foregroundAllowlistConfirmationController
            ?? throw new InvalidOperationException("Foreground allowlist confirmation is not available.");
    }

    private bool AllowEnable()
    {
        if (configurationController is null || !enableApplicationSafety)
        {
            return true;
        }

        ApplicationSafetyDecision decision = ApplicationSafetyPolicy.EvaluateEnable(configurationController.Current);
        applicationSafetyStatusMessage = decision.Message;
        if (!decision.Allowed)
        {
            diagnosticRecorder.RecordSafetyBlockedEnable(
                decision.Message,
                CreateCapturedIdentity(decision));
        }

        return decision.Allowed;
    }

    private static DiagnosticCapturedIdentity? CreateCapturedIdentity(ApplicationSafetyDecision decision)
    {
        ApplicationIdentity? identity = decision.TargetIdentity;
        ApplicationSafetyEntry? rule = decision.MatchedProtectedDenyRule ?? decision.MatchedAllowlistEntry;
        string? ruleName = rule?.DisplayName ?? decision.Classification?.MatchedGameCandidateRule;
        if (identity is null && ruleName is null)
        {
            return null;
        }

        return new DiagnosticCapturedIdentity(
            identity?.ProcessName,
            identity?.WindowTitleContains,
            ruleName);
    }

    private static RuntimeTargetSelector? CreateTargetSelector(TargetWindowInfo foregroundWindow)
    {
        ApplicationIdentity? identity = ApplicationIdentity.FromTargetWindow(foregroundWindow);
        if (identity is not null)
        {
            return RuntimeTargetSelector.ForApplicationIdentity(identity);
        }

        return null;
    }
}
