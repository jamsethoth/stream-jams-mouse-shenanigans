namespace MouseShenanigans.Windows;

public sealed record ApplicationSelfExitDecision(
    bool ShouldExit,
    ProcessSnapshot? MatchedProcess,
    ApplicationSafetyEntry? MatchedSelfExitEntry,
    string Message)
{
    public ApplicationSafetyEntry? MatchedProtectedDenyRule => MatchedSelfExitEntry;

    public static ApplicationSelfExitDecision Continue { get; } = new(
        ShouldExit: false,
        MatchedProcess: null,
        MatchedSelfExitEntry: null,
        Message: "No configured self-exit application is running.");

    public static ApplicationSelfExitDecision Exit(
        ProcessSnapshot matchedProcess,
        ApplicationSafetyEntry matchedSelfExitEntry)
    {
        return Exit(
            matchedProcess,
            matchedSelfExitEntry,
            $"running process '{CreateProcessDisplayName(matchedProcess)}' matched '{matchedSelfExitEntry.DisplayName}'");
    }

    public static ApplicationSelfExitDecision Exit(
        ProcessSnapshot matchedProcess,
        ApplicationSafetyEntry? matchedSelfExitEntry,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(matchedProcess);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new ApplicationSelfExitDecision(
            ShouldExit: true,
            matchedProcess,
            matchedSelfExitEntry,
            $"Self-exit requested because {reason}.");
    }

    public static ApplicationSelfExitDecision ExitForInspectionFailure(string message)
    {
        return new ApplicationSelfExitDecision(
            ShouldExit: true,
            MatchedProcess: null,
            MatchedSelfExitEntry: null,
            $"Self-exit requested because process inspection failed: {message}");
    }

    private static string CreateProcessDisplayName(ProcessSnapshot process)
    {
        return process.Identity?.DisplayName ?? $"process {process.ProcessId}";
    }
}
