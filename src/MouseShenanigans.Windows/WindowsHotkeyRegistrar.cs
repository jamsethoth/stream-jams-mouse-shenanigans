using System.ComponentModel;
using System.Windows.Forms;

namespace MouseShenanigans.Windows;

public sealed class WindowsHotkeyRegistrar : IHotkeyRegistrar
{
    private const int FirstHotkeyId = 0x4D50;

    private readonly IWindowsHotkeyNativeApi nativeApi;
    private readonly Dictionary<int, HotkeyBinding> registeredBindings = [];
    private IntPtr registeredWindowHandle;
    private int nextHotkeyId = FirstHotkeyId;
    private bool disposed;

    public WindowsHotkeyRegistrar()
        : this(new WindowsHotkeyNativeApi())
    {
    }

    public WindowsHotkeyRegistrar(IWindowsHotkeyNativeApi nativeApi)
    {
        this.nativeApi = nativeApi ?? throw new ArgumentNullException(nameof(nativeApi));
    }

    public HotkeyRegistrationResult Register(IntPtr windowHandle, IReadOnlyCollection<HotkeyBinding> bindings)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(bindings);

        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("A hotkey message window handle is required.", nameof(windowHandle));
        }

        HotkeyBindingValidator.Validate(bindings);
        UnregisterAll();
        registeredWindowHandle = windowHandle;

        var registered = new List<HotkeyBinding>();
        var failures = new List<HotkeyRegistrationFailure>();
        foreach (HotkeyBinding binding in bindings)
        {
            int id = nextHotkeyId++;
            if (nativeApi.RegisterHotKey(windowHandle, id, ToNativeModifiers(binding.Modifiers), ToVirtualKeyCode(binding.Key)))
            {
                registeredBindings.Add(id, binding);
                registered.Add(binding);
                continue;
            }

            int errorCode = nativeApi.GetLastError();
            failures.Add(new HotkeyRegistrationFailure(
                binding,
                errorCode,
                new Win32Exception(errorCode).Message));
        }

        return new HotkeyRegistrationResult(registered, failures);
    }

    public RuntimeCommand? TryResolveCommand(int hotkeyId)
    {
        return registeredBindings.TryGetValue(hotkeyId, out HotkeyBinding? binding)
            ? binding.Command
            : null;
    }

    public void UnregisterAll()
    {
        foreach (int id in registeredBindings.Keys.ToArray())
        {
            nativeApi.UnregisterHotKey(registeredWindowHandle, id);
        }

        registeredBindings.Clear();
        registeredWindowHandle = IntPtr.Zero;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        UnregisterAll();
        disposed = true;
    }

    private static uint ToNativeModifiers(HotkeyModifiers modifiers)
    {
        return (uint)modifiers;
    }

    private static uint ToVirtualKeyCode(Keys key)
    {
        return (uint)key;
    }
}
