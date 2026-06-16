using Xunit;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

public sealed class WindowsIntegrationFactAttribute : FactAttribute
{
    public WindowsIntegrationFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Windows integration tests require Windows.";
        }
    }
}
