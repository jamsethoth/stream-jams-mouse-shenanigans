using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class LocalControlEndpointHandler
{
    private readonly RuntimeCommandController commandController;
    private readonly IDiagnosticRecorder diagnosticRecorder;
    private readonly Func<string?> getDegradedStatusMessage;
    private readonly Action requestStatusRefresh;
    private readonly Func<Func<LocalControlEndpointResult>, LocalControlEndpointResult> runRequestOnControlThread;

    public LocalControlEndpointHandler(
        RuntimeCommandController commandController,
        IDiagnosticRecorder? diagnosticRecorder = null,
        Func<string?>? getDegradedStatusMessage = null,
        Action? requestStatusRefresh = null,
        Func<Func<LocalControlEndpointResult>, LocalControlEndpointResult>? runRequestOnControlThread = null)
    {
        this.commandController = commandController ?? throw new ArgumentNullException(nameof(commandController));
        this.diagnosticRecorder = diagnosticRecorder ?? NullDiagnosticRecorder.Instance;
        this.getDegradedStatusMessage = getDegradedStatusMessage ?? (() => null);
        this.requestStatusRefresh = requestStatusRefresh ?? (() => { });
        this.runRequestOnControlThread = runRequestOnControlThread ?? (operation => operation());
    }

    public LocalControlEndpointResult GetStatus()
    {
        return runRequestOnControlThread(() => LocalControlEndpointResult.Ok(CreateSnapshot()));
    }

    public LocalControlEndpointResult GetDiagnostics()
    {
        return LocalControlEndpointResult.Ok(new LocalControlDiagnosticsResponse(
            Ok: true,
            Events: diagnosticRecorder.Snapshot()));
    }

    public LocalControlEndpointResult GetProfiles()
    {
        return runRequestOnControlThread(() =>
        {
            RuntimeConfiguration? configuration = commandController.CurrentConfiguration;
            return LocalControlEndpointResult.Ok(new LocalControlProfilesResponse(
                Ok: true,
                ActiveProfile: configuration?.ActiveProfileName ?? RuntimeProofOfConceptDefaults.ActiveProfileName,
                Profiles: configuration?.ProfileNames ?? [RuntimeProofOfConceptDefaults.ActiveProfileName],
                Message: CreateMessage()));
        });
    }

    public LocalControlEndpointResult Execute(RuntimeCommand command)
    {
        return runRequestOnControlThread(() =>
        {
            commandController.Execute(command);
            requestStatusRefresh();
            return LocalControlEndpointResult.Ok(CreateSnapshot());
        });
    }

    public LocalControlEndpointResult CaptureForegroundTarget()
    {
        return runRequestOnControlThread(CaptureForegroundTargetCore);
    }

    public LocalControlEndpointResult SelectProfile(LocalControlSelectProfileRequest? request)
    {
        return runRequestOnControlThread(() => SelectProfileCore(request));
    }

    public LocalControlEndpointResult ReloadConfiguration()
    {
        return runRequestOnControlThread(ReloadConfigurationCore);
    }

    public LocalControlRuntimeSnapshotResponse CreateSnapshot()
    {
        RuntimeRemappingStatus status = commandController.RuntimeStatus;
        RuntimeConfiguration? configuration = commandController.CurrentConfiguration;

        return new LocalControlRuntimeSnapshotResponse(
            Ok: true,
            State: status.State.ToString().ToLowerInvariant(),
            CursorLockEnabled: commandController.IsCursorLockEnabled,
            Target: configuration?.TargetDisplayName,
            ActiveProfile: configuration?.ActiveProfileName ?? RuntimeProofOfConceptDefaults.ActiveProfileName,
            Profiles: configuration?.ProfileNames ?? [RuntimeProofOfConceptDefaults.ActiveProfileName],
            Message: CreateMessage());
    }

    private LocalControlEndpointResult CaptureForegroundTargetCore()
    {
        diagnosticRecorder.Record(
            DiagnosticEventTypes.ForegroundCaptureRequested,
            "Foreground target capture requested.");

        try
        {
            RuntimeConfigurationOperationResult result = commandController.CaptureForegroundTarget();
            requestStatusRefresh();

            if (!result.Succeeded)
            {
                diagnosticRecorder.Record(
                    DiagnosticEventTypes.ForegroundCaptureFailed,
                    result.Message ?? "Target capture failed.");
                string errorCode = IsTargetCaptureFailure(result.Message)
                    ? LocalControlErrorCodes.TargetCaptureFailed
                    : LocalControlErrorCodes.ConfigurationSaveFailed;
                return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                    Ok: false,
                    Error: errorCode,
                    Message: result.Message ?? "Target capture failed."));
            }

            RuntimeTargetSelector selector = result.Configuration.TargetSelector;
            diagnosticRecorder.Record(
                DiagnosticEventTypes.ForegroundCaptureAccepted,
                "Foreground target capture completed.",
                new DiagnosticCapturedIdentity(
                    ProcessName: selector.ProcessName,
                    WindowTitle: selector.WindowTitleContains));
            return LocalControlEndpointResult.Ok(CreateSnapshot());
        }
        catch (InvalidOperationException ex)
        {
            diagnosticRecorder.Record(DiagnosticEventTypes.ForegroundCaptureFailed, ex.Message);
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.ConfigurationUnavailable,
                Message: ex.Message));
        }
    }

    private LocalControlEndpointResult SelectProfileCore(LocalControlSelectProfileRequest? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.MissingProfileName,
                Message: "Profile name is required."));
        }

        try
        {
            RuntimeConfigurationOperationResult result = commandController.SelectProfile(request.Name);
            requestStatusRefresh();

            if (!result.Succeeded)
            {
                return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                    Ok: false,
                    Error: LocalControlErrorCodes.ConfigurationSaveFailed,
                    Message: result.Message ?? "Configuration save failed."));
            }

            return LocalControlEndpointResult.Ok(CreateSnapshot());
        }
        catch (KeyNotFoundException)
        {
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.ProfileNotFound,
                Message: $"Profile '{request.Name}' was not found."));
        }
        catch (ArgumentException ex)
        {
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.MissingProfileName,
                Message: ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.ConfigurationUnavailable,
                Message: ex.Message));
        }
    }

    private LocalControlEndpointResult ReloadConfigurationCore()
    {
        try
        {
            RuntimeConfigurationOperationResult result = commandController.ReloadConfiguration();
            requestStatusRefresh();

            if (!result.Succeeded)
            {
                return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                    Ok: false,
                    Error: LocalControlErrorCodes.ConfigurationReloadFailed,
                    Message: result.Message ?? "Configuration reload failed."));
            }

            return LocalControlEndpointResult.Ok(CreateSnapshot());
        }
        catch (InvalidOperationException ex)
        {
            return LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                Ok: false,
                Error: LocalControlErrorCodes.ConfigurationUnavailable,
                Message: ex.Message));
        }
    }

    private string? CreateMessage()
    {
        string?[] messages =
        [
            commandController.RuntimeStatus.Message,
            commandController.ConfigurationStatusMessage,
            getDegradedStatusMessage(),
        ];

        string[] nonEmptyMessages = messages
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message!)
            .ToArray();

        return nonEmptyMessages.Length == 0
            ? null
            : string.Join("; ", nonEmptyMessages);
    }

    private static bool IsTargetCaptureFailure(string? message)
    {
        return message?.StartsWith("Target capture failed:", StringComparison.Ordinal) == true;
    }
}
