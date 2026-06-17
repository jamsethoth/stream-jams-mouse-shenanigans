using MouseShenanigans.Windows;
using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class TrayAppHarnessTests
{
    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public void LaunchOptionsUseIsolatedConfigDiagnosticsAndLoopbackLocalControl()
    {
        using TemporaryDirectory directory = TemporaryDirectory.Create("launch-options");
        using ReservedLoopbackPort port = ReservedLoopbackPort.Reserve();

        TrayAppLaunchOptions options = TrayAppLaunchOptions.Create(directory.DirectoryPath, port.BaseUri);

        Assert.StartsWith(directory.DirectoryPath, options.ConfigurationPath, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(directory.DirectoryPath, options.DiagnosticsPath, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(RuntimeConfigurationFileStore.CreateDefaultConfigurationPath(), options.ConfigurationPath);
        Assert.True(options.LocalControlBaseUri.IsLoopback);
        Assert.Equal("http", options.LocalControlBaseUri.Scheme);
    }
}
