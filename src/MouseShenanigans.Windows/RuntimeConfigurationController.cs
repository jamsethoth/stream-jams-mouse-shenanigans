namespace MouseShenanigans.Windows;

public sealed class RuntimeConfigurationController
{
    private readonly IRuntimeConfigurationStore store;
    private readonly IDiagnosticRecorder diagnosticRecorder;

    public RuntimeConfigurationController(
        IRuntimeConfigurationStore store,
        RuntimeConfiguration fallbackConfiguration,
        IDiagnosticRecorder? diagnosticRecorder = null)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.diagnosticRecorder = diagnosticRecorder ?? NullDiagnosticRecorder.Instance;
        RuntimeConfigurationLoadResult loadResult = store.LoadOrFallback(
            fallbackConfiguration ?? throw new ArgumentNullException(nameof(fallbackConfiguration)));

        Current = loadResult.Configuration;
        StatusMessage = loadResult.ErrorMessage is null
            ? null
            : $"Configuration fallback active: {loadResult.ErrorMessage}";

        if (loadResult.ErrorMessage is not null)
        {
            this.diagnosticRecorder.Record(
                DiagnosticEventTypes.ConfigurationLoadFallback,
                StatusMessage!);
        }
    }

    public RuntimeConfiguration Current { get; private set; }

    public string? StatusMessage { get; private set; }

    public string ConfigurationPath => store.ConfigurationPath;

    public RuntimeConfigurationOperationResult SelectProfile(string profileName)
    {
        RuntimeConfiguration updatedConfiguration = Current.WithActiveProfile(profileName);
        return SaveCurrent(updatedConfiguration);
    }

    public RuntimeConfigurationOperationResult SelectTarget(RuntimeTargetSelector targetSelector)
    {
        RuntimeConfiguration updatedConfiguration = Current.WithTargetSelector(targetSelector);
        return SaveCurrent(updatedConfiguration);
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
            diagnosticRecorder.Record(DiagnosticEventTypes.ConfigurationReloadFailed, StatusMessage);
            return new RuntimeConfigurationOperationResult(Current, Succeeded: false, StatusMessage);
        }
    }

    public RuntimeConfigurationOperationResult ReportOperationFailure(string message)
    {
        StatusMessage = message;
        return new RuntimeConfigurationOperationResult(Current, Succeeded: false, StatusMessage);
    }

    private RuntimeConfigurationOperationResult SaveCurrent(RuntimeConfiguration updatedConfiguration)
    {
        Current = updatedConfiguration;
        try
        {
            store.Save(updatedConfiguration);
            StatusMessage = null;
            return new RuntimeConfigurationOperationResult(updatedConfiguration, Succeeded: true, Message: null);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"Configuration save failed: {exception.Message}";
            diagnosticRecorder.Record(DiagnosticEventTypes.ConfigurationSaveFailed, StatusMessage);
            return new RuntimeConfigurationOperationResult(updatedConfiguration, Succeeded: false, StatusMessage);
        }
    }
}
