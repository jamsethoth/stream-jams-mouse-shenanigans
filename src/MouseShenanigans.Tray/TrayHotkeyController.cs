using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayHotkeyController : IDisposable
{
    private readonly IHotkeyRegistrar registrar;
    private readonly RuntimeCommandController commandController;
    private readonly Action refreshStatus;
    private bool disposed;

    public TrayHotkeyController(
        IHotkeyRegistrar registrar,
        RuntimeCommandController commandController,
        Action refreshStatus)
    {
        this.registrar = registrar ?? throw new ArgumentNullException(nameof(registrar));
        this.commandController = commandController ?? throw new ArgumentNullException(nameof(commandController));
        this.refreshStatus = refreshStatus ?? throw new ArgumentNullException(nameof(refreshStatus));
    }

    public HotkeyRegistrationResult RegistrationResult { get; private set; } = HotkeyRegistrationResult.Success;

    public int? LastReceivedHotkeyId { get; private set; }

    public RuntimeCommand? LastDispatchedCommand { get; private set; }

    public HotkeyRegistrationResult Register(IntPtr windowHandle, IReadOnlyCollection<HotkeyBinding> bindings)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        RegistrationResult = registrar.Register(windowHandle, bindings);
        return RegistrationResult;
    }

    public bool DispatchHotkey(int hotkeyId)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        LastReceivedHotkeyId = hotkeyId;
        RuntimeCommand? command = registrar.TryResolveCommand(hotkeyId);
        if (command is not { } runtimeCommand)
        {
            return false;
        }

        LastDispatchedCommand = runtimeCommand;
        commandController.Execute(runtimeCommand);
        refreshStatus();
        return true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        registrar.Dispose();
        disposed = true;
    }
}
