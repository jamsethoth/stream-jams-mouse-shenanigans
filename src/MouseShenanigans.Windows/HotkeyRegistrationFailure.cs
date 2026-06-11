namespace MouseShenanigans.Windows;

public sealed record HotkeyRegistrationFailure(
    HotkeyBinding Binding,
    int NativeErrorCode,
    string Message);
