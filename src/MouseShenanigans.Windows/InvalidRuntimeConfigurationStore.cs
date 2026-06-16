namespace MouseShenanigans.Windows;

public sealed class InvalidRuntimeConfigurationStore : IRuntimeConfigurationStore
{
    private readonly string message;

    public InvalidRuntimeConfigurationStore(string configurationPath, string message)
    {
        ConfigurationPath = string.IsNullOrWhiteSpace(configurationPath) ? "<invalid override>" : configurationPath;
        this.message = string.IsNullOrWhiteSpace(message)
            ? "Runtime configuration path override is invalid."
            : message;
    }

    public string ConfigurationPath { get; }

    public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fallbackConfiguration);

        return new RuntimeConfigurationLoadResult(fallbackConfiguration, UsedFallback: true, ErrorMessage: message);
    }

    public RuntimeConfiguration LoadRequired()
    {
        throw new InvalidDataException(message);
    }

    public void Save(RuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        throw new InvalidDataException(message);
    }
}
