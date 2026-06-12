namespace MouseShenanigans.Windows;

public sealed class RuntimeCommandController
{
    private readonly IRuntimeRemappingController runtime;
    private readonly RuntimeConfigurationController? configurationController;

    public RuntimeCommandController(
        IRuntimeRemappingController runtime,
        RuntimeConfigurationController? configurationController = null)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        this.configurationController = configurationController;
    }

    public RuntimeConfiguration? CurrentConfiguration => configurationController?.Current;

    public string? ConfigurationStatusMessage => configurationController?.StatusMessage;

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

    private RuntimeConfigurationController GetConfigurationController()
    {
        return configurationController
            ?? throw new InvalidOperationException("Runtime configuration is not available.");
    }
}
