using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeConfigurationFileStoreTests
{
    [Fact]
    public void MissingConfigReturnsFallbackAndCreatesEditableDefaultConfig()
    {
        string path = CreateTempConfigPath();
        var store = new RuntimeConfigurationFileStore(new FixedPathProvider(path));
        RuntimeConfiguration fallback = RuntimeProofOfConceptDefaults.CreateConfiguration();

        RuntimeConfigurationLoadResult result = store.LoadOrFallback(fallback);

        Assert.True(result.UsedFallback);
        Assert.Null(result.ErrorMessage);
        Assert.Same(fallback, result.Configuration);
        Assert.True(File.Exists(path));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);

        RuntimeConfiguration loaded = store.LoadRequired();
        Assert.Equal(fallback.ActiveProfileName, loaded.ActiveProfileName);
        Assert.Equal(fallback.TargetSelector.ProcessName, loaded.TargetSelector.ProcessName);
        Assert.Equal(fallback.CursorLockEnabled, loaded.CursorLockEnabled);
    }

    [Fact]
    public void MissingConfigCreationFailureStillReturnsFallbackWithDiagnostic()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            "MouseShenanigans.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        var store = new RuntimeConfigurationFileStore(new FixedPathProvider(path));
        RuntimeConfiguration fallback = RuntimeProofOfConceptDefaults.CreateConfiguration();

        RuntimeConfigurationLoadResult result = store.LoadOrFallback(fallback);

        Assert.True(result.UsedFallback);
        Assert.Same(fallback, result.Configuration);
        Assert.Contains("could not be created", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidConfigReturnsFallbackWithDiagnostic()
    {
        string path = CreateTempConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{", System.Text.Encoding.UTF8);
        var store = new RuntimeConfigurationFileStore(new FixedPathProvider(path));
        RuntimeConfiguration fallback = RuntimeProofOfConceptDefaults.CreateConfiguration();

        RuntimeConfigurationLoadResult result = store.LoadOrFallback(fallback);

        Assert.True(result.UsedFallback);
        Assert.Same(fallback, result.Configuration);
        Assert.Contains("malformed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SaveWritesUtf8JsonAndLoadReadsIt()
    {
        string path = CreateTempConfigPath();
        var store = new RuntimeConfigurationFileStore(new FixedPathProvider(path));
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration();

        store.Save(configuration);

        byte[] bytes = File.ReadAllBytes(path);
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);

        RuntimeConfiguration loaded = store.LoadRequired();
        Assert.Equal(configuration.ActiveProfileName, loaded.ActiveProfileName);
        Assert.Equal(configuration.TargetSelector.ProcessName, loaded.TargetSelector.ProcessName);
    }

    private static string CreateTempConfigPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "MouseShenanigans.Tests",
            Guid.NewGuid().ToString("N"),
            "config.json");
    }

    private sealed class FixedPathProvider(string path) : IRuntimeConfigurationPathProvider
    {
        public string GetConfigurationPath()
        {
            return path;
        }
    }
}
