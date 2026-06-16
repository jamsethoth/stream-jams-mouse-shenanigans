using System.Diagnostics;

namespace MouseShenanigans.Windows;

public sealed class ApplicationSafetySentinel
{
    private readonly Func<RuntimeConfiguration> getConfiguration;
    private readonly IProcessSnapshotReader processSnapshotReader;
    private readonly Action emergencyDisable;
    private readonly Action requestExit;
    private readonly Func<bool> isRuntimeEnabled;
    private readonly IDiagnosticRecorder diagnosticRecorder;
    private bool exitRequested;

    public ApplicationSafetySentinel(
        Func<RuntimeConfiguration> getConfiguration,
        IProcessSnapshotReader processSnapshotReader,
        Action emergencyDisable,
        Action requestExit,
        Func<bool>? isRuntimeEnabled = null,
        IDiagnosticRecorder? diagnosticRecorder = null)
    {
        this.getConfiguration = getConfiguration ?? throw new ArgumentNullException(nameof(getConfiguration));
        this.processSnapshotReader = processSnapshotReader ?? throw new ArgumentNullException(nameof(processSnapshotReader));
        this.emergencyDisable = emergencyDisable ?? throw new ArgumentNullException(nameof(emergencyDisable));
        this.requestExit = requestExit ?? throw new ArgumentNullException(nameof(requestExit));
        this.isRuntimeEnabled = isRuntimeEnabled ?? (() => true);
        this.diagnosticRecorder = diagnosticRecorder ?? NullDiagnosticRecorder.Instance;
    }

    public string? StatusMessage { get; private set; }

    public ApplicationSelfExitDecision EvaluateOnce()
    {
        if (exitRequested)
        {
            return ApplicationSelfExitDecision.Continue;
        }

        RuntimeConfiguration configuration = getConfiguration();
        bool runtimeEnabled = isRuntimeEnabled();
        if (!configuration.Safety.ShouldInspectProcesses(runtimeEnabled))
        {
            return ApplicationSelfExitDecision.Continue;
        }

        IReadOnlyList<ProcessSnapshot> runningProcesses;
        try
        {
            runningProcesses = processSnapshotReader.ReadProcesses();
        }
        catch (Exception ex)
        {
            string message = $"Self-exit monitor inspection failed: {ex.Message}";
            StatusMessage = message;
            Trace.TraceInformation(message);
            if (!runtimeEnabled)
            {
                return ApplicationSelfExitDecision.Continue;
            }

            ApplicationSelfExitDecision failureDecision =
                ApplicationSelfExitDecision.ExitForInspectionFailure(ex.Message);
            RequestExit(failureDecision);
            return failureDecision;
        }

        ApplicationSelfExitDecision decision = ApplicationSafetyPolicy.EvaluateSelfExit(
            configuration.Safety,
            runningProcesses,
            runtimeEnabled);
        if (!decision.ShouldExit)
        {
            return decision;
        }

        RequestExit(decision);
        return decision;
    }

    private void RequestExit(ApplicationSelfExitDecision decision)
    {
        exitRequested = true;
        StatusMessage = decision.Message;
        Trace.TraceInformation(decision.Message);
        diagnosticRecorder.RecordSelfExitRequested(
            decision.Message,
            CreateCapturedIdentity(decision));
        emergencyDisable();
        requestExit();
    }

    private static DiagnosticCapturedIdentity? CreateCapturedIdentity(ApplicationSelfExitDecision decision)
    {
        ApplicationIdentity? identity = decision.MatchedProcess?.Identity;
        ApplicationSafetyEntry? rule = decision.MatchedProtectedDenyRule;
        if (identity is null && rule is null)
        {
            return null;
        }

        return new DiagnosticCapturedIdentity(
            identity?.ProcessName,
            identity?.WindowTitleContains,
            rule?.DisplayName);
    }
}
