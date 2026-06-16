using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayStartupOptions
{
    public const string RuntimeConfigurationPathEnvironmentVariable = "MOUSE_SHENANIGANS_CONFIG_PATH";
    public const string LocalControlUrlEnvironmentVariable = "MOUSE_SHENANIGANS_LOCAL_CONTROL_URL";
    public const string DiagnosticsPathEnvironmentVariable = "MOUSE_SHENANIGANS_DIAGNOSTICS_PATH";
    public const string SelfExitSentinelIntervalEnvironmentVariable = "MOUSE_SHENANIGANS_SELF_EXIT_SENTINEL_INTERVAL_MS";

    public static readonly TimeSpan DefaultSelfExitSentinelInterval = TimeSpan.FromSeconds(30);

    private TrayStartupOptions(
        string? runtimeConfigurationPath,
        string? runtimeConfigurationPathError,
        LocalControlOptions? localControlOptions,
        string? localControlUrlError,
        string? diagnosticsPath,
        string? diagnosticsPathError,
        TimeSpan selfExitSentinelInterval,
        string? selfExitSentinelIntervalError)
    {
        RuntimeConfigurationPath = runtimeConfigurationPath;
        RuntimeConfigurationPathError = runtimeConfigurationPathError;
        LocalControlOptions = localControlOptions;
        LocalControlUrlError = localControlUrlError;
        DiagnosticsPath = diagnosticsPath;
        DiagnosticsPathError = diagnosticsPathError;
        SelfExitSentinelInterval = selfExitSentinelInterval;
        SelfExitSentinelIntervalError = selfExitSentinelIntervalError;
        ValidationMessages = CreateValidationMessages();
    }

    public string? RuntimeConfigurationPath { get; }

    public string? RuntimeConfigurationPathError { get; }

    public LocalControlOptions? LocalControlOptions { get; }

    public string? LocalControlUrlError { get; }

    public string? DiagnosticsPath { get; }

    public string? DiagnosticsPathError { get; }

    public TimeSpan SelfExitSentinelInterval { get; }

    public string? SelfExitSentinelIntervalError { get; }

    public IReadOnlyList<string> ValidationMessages { get; }

    public string? ValidationMessage => ValidationMessages.Count == 0
        ? null
        : string.Join("; ", ValidationMessages);

    public bool HasInvalidRuntimeConfigurationPathOverride => RuntimeConfigurationPathError is not null;

    public bool HasInvalidLocalControlUrlOverride => LocalControlUrlError is not null;

    public static TrayStartupOptions Default { get; } = new(
        runtimeConfigurationPath: null,
        runtimeConfigurationPathError: null,
        localControlOptions: LocalControlOptions.Default,
        localControlUrlError: null,
        diagnosticsPath: null,
        diagnosticsPathError: null,
        selfExitSentinelInterval: DefaultSelfExitSentinelInterval,
        selfExitSentinelIntervalError: null);

    public static TrayStartupOptions FromEnvironment()
    {
        return FromEnvironment(Environment.GetEnvironmentVariable);
    }

    public static TrayStartupOptions FromEnvironment(Func<string, string?> getEnvironmentVariable)
    {
        ArgumentNullException.ThrowIfNull(getEnvironmentVariable);

        (string? runtimeConfigurationPath, string? runtimeConfigurationPathError) = ValidateFilePathOverride(
            RuntimeConfigurationPathEnvironmentVariable,
            getEnvironmentVariable(RuntimeConfigurationPathEnvironmentVariable));
        (string? diagnosticsPath, string? diagnosticsPathError) = ValidateFilePathOverride(
            DiagnosticsPathEnvironmentVariable,
            getEnvironmentVariable(DiagnosticsPathEnvironmentVariable));
        (LocalControlOptions? localControlOptions, string? localControlUrlError) = ValidateLocalControlUrlOverride(
            getEnvironmentVariable(LocalControlUrlEnvironmentVariable));
        (TimeSpan selfExitSentinelInterval, string? selfExitSentinelIntervalError) =
            ValidateSelfExitSentinelIntervalOverride(getEnvironmentVariable(SelfExitSentinelIntervalEnvironmentVariable));

        return new TrayStartupOptions(
            runtimeConfigurationPath,
            runtimeConfigurationPathError,
            localControlOptions,
            localControlUrlError,
            diagnosticsPath,
            diagnosticsPathError,
            selfExitSentinelInterval,
            selfExitSentinelIntervalError);
    }

    private string[] CreateValidationMessages()
    {
        string?[] messages =
        [
            RuntimeConfigurationPathError,
            LocalControlUrlError,
            DiagnosticsPathError,
            SelfExitSentinelIntervalError,
        ];

        return messages
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message!)
            .ToArray();
    }

    private static (string? Path, string? Error) ValidateFilePathOverride(string environmentVariable, string? value)
    {
        if (value is null)
        {
            return (null, null);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, $"{environmentVariable} must name a configuration file path, not an empty value.");
        }

        string trimmed = value.Trim();
        try
        {
            if (!Path.IsPathFullyQualified(trimmed))
            {
                return (null, $"{environmentVariable} must be a fully qualified file path.");
            }

            string fullPath = Path.GetFullPath(trimmed);
            if (trimmed.EndsWith(Path.DirectorySeparatorChar)
                || trimmed.EndsWith(Path.AltDirectorySeparatorChar)
                || string.IsNullOrWhiteSpace(Path.GetFileName(fullPath))
                || Directory.Exists(fullPath))
            {
                return (null, $"{environmentVariable} must point to a file path, not a directory.");
            }

            return (fullPath, null);
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException)
        {
            return (null, $"{environmentVariable} is not a valid file path: {exception.Message}");
        }
    }

    private static (LocalControlOptions? Options, string? Error) ValidateLocalControlUrlOverride(string? value)
    {
        if (value is null)
        {
            return (LocalControlOptions.Default, null);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, $"{LocalControlUrlEnvironmentVariable} must be an absolute HTTP loopback URL.");
        }

        try
        {
            return (LocalControlOptions.Create(value.Trim()), null);
        }
        catch (ArgumentException exception)
        {
            return (null, $"{LocalControlUrlEnvironmentVariable} is invalid: {exception.Message}");
        }
    }

    private static (TimeSpan Interval, string? Error) ValidateSelfExitSentinelIntervalOverride(string? value)
    {
        if (value is null)
        {
            return (DefaultSelfExitSentinelInterval, null);
        }

        if (!int.TryParse(value.Trim(), out int milliseconds) || milliseconds <= 0)
        {
            return (
                DefaultSelfExitSentinelInterval,
                $"{SelfExitSentinelIntervalEnvironmentVariable} must be a positive integer number of milliseconds.");
        }

        return (TimeSpan.FromMilliseconds(milliseconds), null);
    }
}
