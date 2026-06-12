namespace MouseShenanigans.Windows;

public sealed class RuntimeCommandController
{
    private readonly IRuntimeRemappingController runtime;

    public RuntimeCommandController(IRuntimeRemappingController runtime)
    {
        this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

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
}
