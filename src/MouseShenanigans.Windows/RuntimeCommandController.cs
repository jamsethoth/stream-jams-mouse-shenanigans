namespace MouseShenanigans.Windows;

public sealed class RuntimeCommandController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly RuntimeConfigurationController? configurationController;
    private readonly ITargetWindowReader? targetWindowReader;

    public RuntimeCommandController(
        IRuntimeRemappingController runtime,
        RuntimeConfigurationController? configurationController = null,
        ITargetWindowReader? targetWindowReader = null)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.configurationController = configurationController;
        this.targetWindowReader = targetWindowReader;
    }

    public RuntimeConfiguration? CurrentConfiguration => configurationController?.Current;

    public string? ConfigurationStatusMessage => configurationController?.StatusMessage;

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
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown runtime command.");
        }
    }

    public void Enable()
    {
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

        runtime.Enable();
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

    private static RuntimeTargetSelector? CreateTargetSelector(TargetWindowInfo foregroundWindow)
    {
        if (!string.IsNullOrWhiteSpace(foregroundWindow.ProcessName))
        {
            return RuntimeTargetSelector.ForProcessName(foregroundWindow.ProcessName);
        }

        return !string.IsNullOrWhiteSpace(foregroundWindow.Title)
            ? RuntimeTargetSelector.ForWindowTitleContains(foregroundWindow.Title)
            : null;
    }
}
