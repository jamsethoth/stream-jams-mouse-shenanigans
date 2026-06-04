namespace MouseShenanigans.Windows;

public static class WindowsRuntime
{
    public static bool IsDesktopInputAvailable => OperatingSystem.IsWindows();
}
