namespace MouseShenanigans.Windows;

public interface IRuntimeConfigurationStore
{
    string ConfigurationPath { get; }

    RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration);

    RuntimeConfiguration LoadRequired();

    void Save(RuntimeConfiguration configuration);
}
