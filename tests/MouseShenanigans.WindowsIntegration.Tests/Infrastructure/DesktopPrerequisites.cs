namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class DesktopPrerequisites
{
    public static DesktopPrerequisiteResult Check()
    {
        if (!OperatingSystem.IsWindows())
        {
            return DesktopPrerequisiteResult.Unsupported("Desktop tests require Windows.");
        }

        if (!Environment.UserInteractive)
        {
            return DesktopPrerequisiteResult.Unsupported("The current process is not running in a desktop-capable user session.");
        }

        IntPtr desktop = NativeMethods.OpenInputDesktop(
            dwFlags: 0,
            fInherit: false,
            dwDesiredAccess: NativeMethods.DesktopSwitchDesktop);
        if (desktop == IntPtr.Zero)
        {
            return DesktopPrerequisiteResult.Unsupported("No input desktop is available to this process.");
        }

        NativeMethods.CloseDesktop(desktop);
        return DesktopPrerequisiteResult.Supported;
    }
}

internal sealed record DesktopPrerequisiteResult(bool IsSupported, string? Reason)
{
    public static DesktopPrerequisiteResult Supported { get; } = new(true, null);

    public static DesktopPrerequisiteResult Unsupported(string reason)
    {
        return new DesktopPrerequisiteResult(false, reason);
    }
}
