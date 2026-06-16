using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class DesktopPrerequisiteTests
{
    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public void DesktopPrerequisiteCheckReportsSupportedOrActionableReason()
    {
        DesktopPrerequisiteResult result = DesktopPrerequisites.Check();

        if (!result.IsSupported)
        {
            Assert.False(string.IsNullOrWhiteSpace(result.Reason));
        }
    }
}
