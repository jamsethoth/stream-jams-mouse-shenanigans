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
        var launcher = new RecordingConfigurationFolderLauncher();
        var refreshRequests = 0;
        var controller = new TrayConfigurationFolderController(
            configurationController,
            launcher,
            () => refreshRequests++);

        controller.OpenConfigurationFolder();

        Assert.True(Directory.Exists(expectedFolderPath));
        Assert.Equal([expectedFolderPath], launcher.OpenedFolders);
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
        var launcher = new ThrowingConfigurationFolderLauncher(new InvalidOperationException("blocked"));
        var refreshRequests = 0;
        var controller = new TrayConfigurationFolderController(
            configurationController,
            launcher,
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

    private sealed class RecordingConfigurationFolderLauncher : IConfigurationFolderLauncher
    {
        public List<string> OpenedFolders { get; } = [];

        public void Open(string folderPath)
        {
            OpenedFolders.Add(folderPath);
        }
    }

    private sealed class ThrowingConfigurationFolderLauncher(Exception exception) : IConfigurationFolderLauncher
    {
        public void Open(string folderPath)
        {
            throw exception;
        }
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
