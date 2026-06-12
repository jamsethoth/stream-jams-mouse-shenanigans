using System.Text;

namespace MouseShenanigans.Windows;

public sealed class RuntimeConfigurationFileStore : IRuntimeConfigurationStore
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly IRuntimeConfigurationPathProvider pathProvider;

    public RuntimeConfigurationFileStore(IRuntimeConfigurationPathProvider pathProvider)
    {
        this.pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    public string ConfigurationPath => pathProvider.GetConfigurationPath();

    public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fallbackConfiguration);

        if (!File.Exists(ConfigurationPath))
        {
            return new RuntimeConfigurationLoadResult(fallbackConfiguration, UsedFallback: true, ErrorMessage: null);
        }

        try
        {
            return new RuntimeConfigurationLoadResult(LoadRequired(), UsedFallback: false, ErrorMessage: null);
        }
        catch (InvalidDataException exception)
        {
            return new RuntimeConfigurationLoadResult(
                fallbackConfiguration,
                UsedFallback: true,
                ErrorMessage: exception.Message);
        }
    }

    public RuntimeConfiguration LoadRequired()
    {
        if (!File.Exists(ConfigurationPath))
        {
            throw new InvalidDataException($"Runtime configuration file was not found at '{ConfigurationPath}'.");
        }

        string json = File.ReadAllText(ConfigurationPath, Encoding.UTF8);
        return RuntimeConfigurationJsonSerializer.Deserialize(json);
    }

    public void Save(RuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? directory = Path.GetDirectoryName(ConfigurationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            ConfigurationPath,
            RuntimeConfigurationJsonSerializer.Serialize(configuration),
            Utf8NoBom);
    }
}
