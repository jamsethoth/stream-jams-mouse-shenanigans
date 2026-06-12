namespace MouseShenanigans.Windows;

public sealed class RuntimeConfigurationController
{
    private readonly IRuntimeConfigurationStore store;

    public RuntimeConfigurationController(
        IRuntimeConfigurationStore store,
        RuntimeConfiguration fallbackConfiguration)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        RuntimeConfigurationLoadResult loadResult = store.LoadOrFallback(
            fallbackConfiguration ?? throw new ArgumentNullException(nameof(fallbackConfiguration)));

        Current = loadResult.Configuration;
        StatusMessage = loadResult.ErrorMessage is null
            ? null
            : $"Configuration fallback active: {loadResult.ErrorMessage}";
    }

    public RuntimeConfiguration Current { get; private set; }

    public string? StatusMessage { get; private set; }

    public string ConfigurationPath => store.ConfigurationPath;

    public RuntimeConfigurationOperationResult SelectProfile(string profileName)
    {
        RuntimeConfiguration updatedConfiguration = Current.WithActiveProfile(profileName);
        Current = updatedConfiguration;
        try
        {
            store.Save(updatedConfiguration);
            StatusMessage = null;
            return new RuntimeConfigurationOperationResult(updatedConfiguration, Succeeded: true, Message: null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"Configuration save failed: {exception.Message}";
            return new RuntimeConfigurationOperationResult(updatedConfiguration, Succeeded: false, StatusMessage);
        }
    }

    public RuntimeConfigurationOperationResult Reload()
    {
        try
        {
            RuntimeConfiguration configuration = store.LoadRequired();
            Current = configuration;
            StatusMessage = null;
            return new RuntimeConfigurationOperationResult(configuration, Succeeded: true, Message: null);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"Configuration reload failed: {exception.Message}";
            return new RuntimeConfigurationOperationResult(Current, Succeeded: false, StatusMessage);
        }
    }
}
