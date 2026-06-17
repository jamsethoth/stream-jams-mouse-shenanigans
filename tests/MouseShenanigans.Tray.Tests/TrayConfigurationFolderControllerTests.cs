using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayConfigurationFolderControllerTests
{
    [Fact]
    public void OpenConfigurationFolderCreatesAndLaunchesConfigDirectory()
    {
        string root = Path.Combine(Path.GetTempPath(), "MouseShenanigans.Tests", Guid.NewGuid().ToString("N"));
        string configPath = Path.Combine(root, "nested", "config.json");
        string expectedFolderPath = Path.GetDirectoryName(configPath)!;
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();
        var configurationController = new RuntimeConfigurationController(
            new RecordingConfigurationStore(configuration, configPath),
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        List<string> openedFolders = [];
        var refreshRequests = 0;
        var controller = new TrayConfigurationFolderController(
            configurationController,
            openedFolders.Add,
            () => refreshRequests++);

        controller.OpenConfigurationFolder();

        Assert.True(Directory.Exists(expectedFolderPath));
        Assert.Equal([expectedFolderPath], openedFolders);
        Assert.Equal(1, refreshRequests);
        Assert.Null(configurationController.StatusMessage);
    }

    [Fact]
    public void OpenConfigurationFolderReportsLauncherFailureAndRefreshesStatus()
    {
        string root = Path.Combine(Path.GetTempPath(), "MouseShenanigans.Tests", Guid.NewGuid().ToString("N"));
        string configPath = Path.Combine(root, "config.json");
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();
        var configurationController = new RuntimeConfigurationController(
            new RecordingConfigurationStore(configuration, configPath),
            RuntimeProofOfConceptDefaults.CreateConfiguration());
        var refreshRequests = 0;
        var controller = new TrayConfigurationFolderController(
            configurationController,
            _ => throw new InvalidOperationException("blocked"),
            () => refreshRequests++);

        controller.OpenConfigurationFolder();

        Assert.Contains("folder open failed", configurationController.StatusMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("blocked", configurationController.StatusMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, refreshRequests);
    }

    [Fact]
    public void GetConfigurationFolderPathRejectsPathWithoutDirectory()
    {
        Assert.Throws<ArgumentException>(() => TrayConfigurationFolderController.GetConfigurationFolderPath("config.json"));
    }

    private sealed class RecordingConfigurationStore(
        RuntimeConfiguration initialConfiguration,
        string configurationPath) : IRuntimeConfigurationStore
    {
        public string ConfigurationPath { get; } = configurationPath;

        public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
        {
            return new RuntimeConfigurationLoadResult(initialConfiguration, UsedFallback: false, ErrorMessage: null);
        }

        public RuntimeConfiguration LoadRequired()
        {
            return initialConfiguration;
        }

        public void Save(RuntimeConfiguration configuration)
        {
        }
    }
}
