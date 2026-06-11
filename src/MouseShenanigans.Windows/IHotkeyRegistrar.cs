namespace MouseShenanigans.Windows;

public interface IHotkeyRegistrar : IDisposable
{
    HotkeyRegistrationResult Register(IntPtr windowHandle, IReadOnlyCollection<HotkeyBinding> bindings);

    RuntimeCommand? TryResolveCommand(int hotkeyId);

    void UnregisterAll();
}
