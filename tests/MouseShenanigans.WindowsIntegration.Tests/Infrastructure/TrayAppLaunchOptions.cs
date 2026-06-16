using System.Diagnostics;
using MouseShenanigans.Tray;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class TrayAppLaunchOptions
{
    private TrayAppLaunchOptions(
        string rootDirectory,
        Uri localControlBaseUri,
        string configurationPath,
        string diagnosticsPath)
    {
        RootDirectory = rootDirectory;
        LocalControlBaseUri = localControlBaseUri;
        ConfigurationPath = configurationPath;
        DiagnosticsPath = diagnosticsPath;
    }

    public string RootDirectory { get; }

    public Uri LocalControlBaseUri { get; }

    public string ConfigurationPath { get; }

    public string DiagnosticsPath { get; }

    public static TrayAppLaunchOptions Create(string rootDirectory, Uri localControlBaseUri)
    {
        ArgumentNullException.ThrowIfNull(localControlBaseUri);

        Directory.CreateDirectory(rootDirectory);
        return new TrayAppLaunchOptions(
            rootDirectory,
            localControlBaseUri,
            Path.Combine(rootDirectory, "runtime-config", "config.json"),
            Path.Combine(rootDirectory, "diagnostics", "diagnostics.jsonl"));
    }

    public void ApplyEnvironment(ProcessStartInfo startInfo)
    {
        startInfo.Environment[TrayStartupOptions.RuntimeConfigurationPathEnvironmentVariable] = ConfigurationPath;
        startInfo.Environment[TrayStartupOptions.LocalControlUrlEnvironmentVariable] = LocalControlBaseUri.ToString();
        startInfo.Environment[TrayStartupOptions.DiagnosticsPathEnvironmentVariable] = DiagnosticsPath;
    }
}
