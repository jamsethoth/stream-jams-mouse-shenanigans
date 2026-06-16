using Xunit;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

public sealed class DesktopFactAttribute : FactAttribute
{
    public DesktopFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Desktop tests require Windows.";
            return;
        }

        if (!IntegrationTestSettings.RunDesktopTests)
        {
            Skip = $"Set {IntegrationTestSettings.RunDesktopTestsEnvironmentVariable}=1 to run desktop tests.";
            return;
        }

        DesktopPrerequisiteResult prerequisites = DesktopPrerequisites.Check();
        if (!prerequisites.IsSupported)
        {
            Skip = prerequisites.Reason;
        }
    }
}
