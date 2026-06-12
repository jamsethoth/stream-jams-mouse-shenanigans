using System.Windows.Forms;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class WindowsHotkeyRegistrarTests
{
    private static readonly IntPtr WindowHandle = new(1234);

    [Fact]
    public void RegisterMapsSuccessfulHotkeyIdsToRuntimeCommands()
    {
        var nativeApi = new RecordingWindowsHotkeyNativeApi();
        using var registrar = new WindowsHotkeyRegistrar(nativeApi);
        HotkeyBinding[] bindings =
        [
            new(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
            new(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, Keys.M),
        ];

        HotkeyRegistrationResult result = registrar.Register(WindowHandle, bindings);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Failures);
        Assert.Equal(RuntimeCommand.ToggleRuntime, registrar.TryResolveCommand(nativeApi.RegisteredIds[0]));
        Assert.Equal(RuntimeCommand.EmergencyDisable, registrar.TryResolveCommand(nativeApi.RegisteredIds[1]));
    }

    [Fact]
    public void RegisterReportsPartialFailureAndKeepsSuccessfulMappings()
    {
        var nativeApi = new RecordingWindowsHotkeyNativeApi
        {
            FailedRegistrationIndex = 1,
            LastError = 1409,
        };
        using var registrar = new WindowsHotkeyRegistrar(nativeApi);
        HotkeyBinding[] bindings =
        [
            new(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
            new(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, Keys.M),
        ];

        HotkeyRegistrationResult result = registrar.Register(WindowHandle, bindings);

        Assert.False(result.Succeeded);
        Assert.Single(result.Failures);
        Assert.Equal(bindings[1], result.Failures[0].Binding);
        Assert.Equal(1409, result.Failures[0].NativeErrorCode);
        Assert.Equal(RuntimeCommand.ToggleRuntime, registrar.TryResolveCommand(nativeApi.RegisteredIds[0]));
        Assert.Null(registrar.TryResolveCommand(nativeApi.FailedIds[0]));
    }

    [Fact]
    public void RegisterUnregistersExistingBindingsBeforeReregistering()
    {
        var nativeApi = new RecordingWindowsHotkeyNativeApi();
        using var registrar = new WindowsHotkeyRegistrar(nativeApi);
        HotkeyBinding first = new(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M);
        HotkeyBinding second = new(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, Keys.M);

        registrar.Register(WindowHandle, [first]);
        int firstId = nativeApi.RegisteredIds[0];
        registrar.Register(WindowHandle, [second]);

        Assert.Equal([(WindowHandle, firstId)], nativeApi.UnregisteredHotkeys);
        Assert.Null(registrar.TryResolveCommand(firstId));
        Assert.Equal(RuntimeCommand.EmergencyDisable, registrar.TryResolveCommand(nativeApi.RegisteredIds[1]));
    }

    [Fact]
    public void DisposeUnregistersRegisteredHotkeysOnce()
    {
        var nativeApi = new RecordingWindowsHotkeyNativeApi();
        var registrar = new WindowsHotkeyRegistrar(nativeApi);
        HotkeyBinding binding = new(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M);
        registrar.Register(WindowHandle, [binding]);
        int id = nativeApi.RegisteredIds[0];

        registrar.Dispose();
        registrar.Dispose();

        Assert.Equal([(WindowHandle, id)], nativeApi.UnregisteredHotkeys);
    }

    [Fact]
    public void RegisterRejectsDuplicateBindingsBeforeNativeRegistration()
    {
        var nativeApi = new RecordingWindowsHotkeyNativeApi();
        using var registrar = new WindowsHotkeyRegistrar(nativeApi);
        HotkeyBinding[] bindings =
        [
            new(RuntimeCommand.ToggleRuntime, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
            new(RuntimeCommand.EmergencyDisable, HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.M),
        ];

        Assert.Throws<ArgumentException>(() => registrar.Register(WindowHandle, bindings));
        Assert.Empty(nativeApi.RegisteredIds);
    }

    private sealed class RecordingWindowsHotkeyNativeApi : IWindowsHotkeyNativeApi
    {
        private int registrationRequests;

        public int? FailedRegistrationIndex { get; init; }

        public int LastError { get; init; }

        public List<int> RegisteredIds { get; } = [];

        public List<int> FailedIds { get; } = [];

        public List<(IntPtr WindowHandle, int Id)> UnregisteredHotkeys { get; } = [];

        public bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKeyCode)
        {
            int currentIndex = registrationRequests++;
            if (FailedRegistrationIndex == currentIndex)
            {
                FailedIds.Add(id);
                return false;
            }

            RegisteredIds.Add(id);
            return true;
        }

        public bool UnregisterHotKey(IntPtr windowHandle, int id)
        {
            UnregisteredHotkeys.Add((windowHandle, id));
            return true;
        }

        public int GetLastError()
        {
            return LastError;
        }
    }
}
