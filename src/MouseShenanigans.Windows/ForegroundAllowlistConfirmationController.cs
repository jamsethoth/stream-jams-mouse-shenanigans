using System.Diagnostics;

namespace MouseShenanigans.Windows;

public sealed class ForegroundAllowlistConfirmationController
{
    private readonly RuntimeConfigurationController configurationController;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly Dictionary<Guid, ForegroundAllowlistConfirmationRequest> requests = [];

    public ForegroundAllowlistConfirmationController(
        RuntimeConfigurationController configurationController,
        ITargetWindowReader targetWindowReader)
    {
        this.configurationController = configurationController ?? throw new ArgumentNullException(nameof(configurationController));
        this.targetWindowReader = targetWindowReader ?? throw new ArgumentNullException(nameof(targetWindowReader));
    }

    public string? StatusMessage { get; private set; }

    public ForegroundAllowlistConfirmationRequestResult RequestForegroundConfirmation(
        ForegroundAllowlistConfirmationSource source)
    {
        TargetWindowInfo? foregroundWindow = targetWindowReader.ReadSnapshot().ForegroundWindow;
        ApplicationIdentity? identity = ApplicationIdentity.FromTargetWindow(foregroundWindow);
        if (identity is null)
        {
            string message = "Foreground allowlist capture failed: no usable foreground window identity was available.";
            StatusMessage = message;
            Trace.TraceInformation(message);
            return new ForegroundAllowlistConfirmationRequestResult(Succeeded: false, Request: null, message);
        }

        var request = new ForegroundAllowlistConfirmationRequest(
            Guid.NewGuid(),
            identity,
            source,
            ForegroundAllowlistConfirmationStatus.Pending);
        requests.Add(request.Id, request);

        string precisionText = identity.ProcessName is null
            ? "title-based"
            : "process";
        string pendingMessage =
            $"Foreground allowlist confirmation pending for {precisionText} identity '{identity.DisplayName}'.";
        StatusMessage = pendingMessage;
        Trace.TraceInformation(pendingMessage);

        return new ForegroundAllowlistConfirmationRequestResult(Succeeded: true, request, pendingMessage);
    }

    public ForegroundAllowlistConfirmationCompletionResult Confirm(Guid requestId)
    {
        if (!requests.TryGetValue(requestId, out ForegroundAllowlistConfirmationRequest? request))
        {
            string missingMessage = $"Foreground allowlist confirmation '{requestId}' was not found.";
            StatusMessage = missingMessage;
            Trace.TraceInformation(missingMessage);
            return new ForegroundAllowlistConfirmationCompletionResult(Succeeded: false, Request: null, missingMessage);
        }

        if (request.Status != ForegroundAllowlistConfirmationStatus.Pending)
        {
            return new ForegroundAllowlistConfirmationCompletionResult(
                Succeeded: true,
                request,
                $"Foreground allowlist confirmation '{requestId}' is already {request.Status}.");
        }

        RuntimeConfigurationOperationResult result = configurationController.AddAllowedApplication(request.Identity);
        if (!result.Succeeded)
        {
            StatusMessage = result.Message;
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Trace.TraceInformation(result.Message);
            }

            return new ForegroundAllowlistConfirmationCompletionResult(
                Succeeded: false,
                request,
                result.Message ?? "Foreground allowlist confirmation could not be persisted.");
        }

        ForegroundAllowlistConfirmationStatus status = result.Configuration.Safety.HasAllowedApplication(request.Identity)
            && result.Message?.Contains("already", StringComparison.OrdinalIgnoreCase) == true
                ? ForegroundAllowlistConfirmationStatus.AlreadyAllowed
                : ForegroundAllowlistConfirmationStatus.Accepted;
        ForegroundAllowlistConfirmationRequest updated = request with { Status = status };
        requests[requestId] = updated;

        string message = status == ForegroundAllowlistConfirmationStatus.AlreadyAllowed
            ? $"Foreground allowlist confirmation already allowed '{request.Identity.DisplayName}'."
            : $"Foreground allowlist confirmation accepted for '{request.Identity.DisplayName}'.";
        StatusMessage = message;
        Trace.TraceInformation(message);
        return new ForegroundAllowlistConfirmationCompletionResult(Succeeded: true, updated, message);
    }

    public ForegroundAllowlistConfirmationCompletionResult Cancel(Guid requestId)
    {
        if (!requests.TryGetValue(requestId, out ForegroundAllowlistConfirmationRequest? request))
        {
            string missingMessage = $"Foreground allowlist confirmation '{requestId}' was not found.";
            StatusMessage = missingMessage;
            Trace.TraceInformation(missingMessage);
            return new ForegroundAllowlistConfirmationCompletionResult(Succeeded: false, Request: null, missingMessage);
        }

        if (request.Status != ForegroundAllowlistConfirmationStatus.Pending)
        {
            return new ForegroundAllowlistConfirmationCompletionResult(
                Succeeded: true,
                request,
                $"Foreground allowlist confirmation '{requestId}' is already {request.Status}.");
        }

        ForegroundAllowlistConfirmationRequest updated = request with
        {
            Status = ForegroundAllowlistConfirmationStatus.Canceled,
        };
        requests[requestId] = updated;
        string message = $"Foreground allowlist confirmation canceled for '{request.Identity.DisplayName}'.";
        StatusMessage = message;
        Trace.TraceInformation(message);
        return new ForegroundAllowlistConfirmationCompletionResult(Succeeded: true, updated, message);
    }
}
