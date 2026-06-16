namespace MouseShenanigans.Tray.Tests;

public sealed class TrayStartupOptionsTests
{
    [Fact]
    public void UnsetOverridesUseProductionDefaults()
    {
        TrayStartupOptions options = TrayStartupOptions.FromEnvironment(_ => null);

        Assert.Null(options.RuntimeConfigurationPath);
        Assert.Same(LocalControlOptions.Default, options.LocalControlOptions);
        Assert.Null(options.DiagnosticsPath);
        Assert.Equal(TrayStartupOptions.DefaultSelfExitSentinelInterval, options.SelfExitSentinelInterval);
        Assert.Empty(options.ValidationMessages);
    }

    [Fact]
    public void ValidOverridesAreParsed()
    {
        string configPath = Path.Combine(Path.GetTempPath(), "MouseShenanigans.Tests", Guid.NewGuid().ToString("N"), "config.json");
        string diagnosticsPath = Path.Combine(Path.GetTempPath(), "MouseShenanigans.Tests", Guid.NewGuid().ToString("N"), "diagnostics.jsonl");
        Dictionary<string, string> environment = new(StringComparer.Ordinal)
        {
            [TrayStartupOptions.RuntimeConfigurationPathEnvironmentVariable] = configPath,
            [TrayStartupOptions.LocalControlUrlEnvironmentVariable] = "http://127.0.0.1:6178",
            [TrayStartupOptions.DiagnosticsPathEnvironmentVariable] = diagnosticsPath,
            [TrayStartupOptions.SelfExitSentinelIntervalEnvironmentVariable] = "125",
        };

        TrayStartupOptions options = TrayStartupOptions.FromEnvironment(name => environment.GetValueOrDefault(name));

        Assert.Equal(Path.GetFullPath(configPath), options.RuntimeConfigurationPath);
        Assert.Equal("http://127.0.0.1:6178", options.LocalControlOptions?.UrlText);
        Assert.Equal(Path.GetFullPath(diagnosticsPath), options.DiagnosticsPath);
        Assert.Equal(TimeSpan.FromMilliseconds(125), options.SelfExitSentinelInterval);
        Assert.Empty(options.ValidationMessages);
    }

    [Fact]
    public void InvalidConfigurationPathDoesNotFallBackToProductionPath()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), "MouseShenanigans.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        Dictionary<string, string> environment = new(StringComparer.Ordinal)
        {
            [TrayStartupOptions.RuntimeConfigurationPathEnvironmentVariable] = directoryPath,
        };

        TrayStartupOptions options = TrayStartupOptions.FromEnvironment(name => environment.GetValueOrDefault(name));

        Assert.True(options.HasInvalidRuntimeConfigurationPathOverride);
        Assert.Null(options.RuntimeConfigurationPath);
        Assert.Contains("file path, not a directory", options.RuntimeConfigurationPathError, StringComparison.Ordinal);
        Assert.Contains(TrayStartupOptions.RuntimeConfigurationPathEnvironmentVariable, options.ValidationMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidLocalControlUrlDisablesLocalControlBinding()
    {
        Dictionary<string, string> environment = new(StringComparer.Ordinal)
        {
            [TrayStartupOptions.LocalControlUrlEnvironmentVariable] = "http://0.0.0.0:5178",
        };

        TrayStartupOptions options = TrayStartupOptions.FromEnvironment(name => environment.GetValueOrDefault(name));

        Assert.True(options.HasInvalidLocalControlUrlOverride);
        Assert.Null(options.LocalControlOptions);
        Assert.Contains("loopback", options.LocalControlUrlError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidSentinelIntervalUsesProductionDefaultWithValidationMessage()
    {
        Dictionary<string, string> environment = new(StringComparer.Ordinal)
        {
            [TrayStartupOptions.SelfExitSentinelIntervalEnvironmentVariable] = "0",
        };

        TrayStartupOptions options = TrayStartupOptions.FromEnvironment(name => environment.GetValueOrDefault(name));

        Assert.Equal(TrayStartupOptions.DefaultSelfExitSentinelInterval, options.SelfExitSentinelInterval);
        Assert.Contains("positive integer", options.SelfExitSentinelIntervalError, StringComparison.Ordinal);
    }
}
