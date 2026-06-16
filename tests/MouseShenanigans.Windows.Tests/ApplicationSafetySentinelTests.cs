using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class ApplicationSafetySentinelTests
{
    [Fact]
    public void EmptySafetyRulesDoNotReadProcessesOrExit()
    {
        var reader = new RecordingProcessSnapshotReader();
        var sentinel = new ApplicationSafetySentinel(
            () => RuntimeProofOfConceptDefaults.CreateConfiguration(),
            reader,
            emergencyDisable: () => throw new InvalidOperationException("should not disable"),
            requestExit: () => throw new InvalidOperationException("should not exit"));

        ApplicationSelfExitDecision decision = sentinel.EvaluateOnce();

        Assert.False(decision.ShouldExit);
        Assert.Equal(0, reader.ReadRequests);
    }

    [Fact]
    public void ProtectedRuleDisablesAndRequestsExitOnce()
    {
        var safety = new ApplicationSafetyConfiguration(
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("GameApp")),
            ]);
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration().WithSafety(safety);
        var reader = new RecordingProcessSnapshotReader
        {
            Processes = [new ProcessSnapshot(101, new ApplicationIdentity("GameApp"))],
        };
        var disableRequests = 0;
        var exitRequests = 0;
        var recorder = new BoundedDiagnosticRecorder();
        var sentinel = new ApplicationSafetySentinel(
            () => configuration,
            reader,
            emergencyDisable: () => disableRequests++,
            requestExit: () => exitRequests++,
            isRuntimeEnabled: () => true,
            diagnosticRecorder: recorder);

        ApplicationSelfExitDecision first = sentinel.EvaluateOnce();
        ApplicationSelfExitDecision second = sentinel.EvaluateOnce();

        Assert.True(first.ShouldExit);
        Assert.False(second.ShouldExit);
        Assert.Equal(1, disableRequests);
        Assert.Equal(1, exitRequests);
        Assert.Contains("Self-exit requested", sentinel.StatusMessage, StringComparison.Ordinal);
        Assert.Equal(DiagnosticEventTypes.SelfExitRequested, recorder.Snapshot().Single().Type);
    }

    [Fact]
    public void ProtectedRuleRequestsExitWhileRuntimeDisabled()
    {
        var safety = new ApplicationSafetyConfiguration(
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("GameApp")),
            ]);
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration().WithSafety(safety);
        var reader = new RecordingProcessSnapshotReader
        {
            Processes = [new ProcessSnapshot(101, new ApplicationIdentity("GameApp"))],
        };
        var exitRequests = 0;
        var sentinel = new ApplicationSafetySentinel(
            () => configuration,
            reader,
            emergencyDisable: () => { },
            requestExit: () => exitRequests++,
            isRuntimeEnabled: () => false);

        ApplicationSelfExitDecision decision = sentinel.EvaluateOnce();

        Assert.True(decision.ShouldExit);
        Assert.Equal(1, exitRequests);
    }

    [Fact]
    public void NonAllowlistedGameCandidateRequestsExitOnlyWhenRuntimeEnabled()
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: ["GameApp"]);
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration().WithSafety(safety);
        var reader = new RecordingProcessSnapshotReader
        {
            Processes = [new ProcessSnapshot(101, new ApplicationIdentity("GameApp"))],
        };
        var exitRequests = 0;
        var sentinel = new ApplicationSafetySentinel(
            () => configuration,
            reader,
            emergencyDisable: () => { },
            requestExit: () => exitRequests++,
            isRuntimeEnabled: () => true);

        ApplicationSelfExitDecision decision = sentinel.EvaluateOnce();

        Assert.True(decision.ShouldExit);
        Assert.Equal(1, exitRequests);
    }

    [Fact]
    public void ProcessInspectionFailureDoesNotRequestExitWhenRuntimeDisabled()
    {
        var safety = new ApplicationSafetyConfiguration(
            protectedGameDenyRules:
            [
                new ApplicationSafetyEntry(new ApplicationIdentity("GameApp")),
            ]);
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration().WithSafety(safety);
        var reader = new RecordingProcessSnapshotReader
        {
            ReadException = new InvalidOperationException("blocked"),
        };
        var exitRequests = 0;
        var sentinel = new ApplicationSafetySentinel(
            () => configuration,
            reader,
            emergencyDisable: () => { },
            requestExit: () => exitRequests++,
            isRuntimeEnabled: () => false);

        ApplicationSelfExitDecision decision = sentinel.EvaluateOnce();

        Assert.False(decision.ShouldExit);
        Assert.Equal(0, exitRequests);
        Assert.Contains("inspection failed", sentinel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProcessInspectionFailureRequestsExitWhenRuntimeEnabled()
    {
        var safety = new ApplicationSafetyConfiguration(gameProcessPatterns: ["GameApp"]);
        RuntimeConfiguration configuration = RuntimeProofOfConceptDefaults.CreateConfiguration().WithSafety(safety);
        var reader = new RecordingProcessSnapshotReader
        {
            ReadException = new InvalidOperationException("blocked"),
        };
        var disableRequests = 0;
        var exitRequests = 0;
        var sentinel = new ApplicationSafetySentinel(
            () => configuration,
            reader,
            emergencyDisable: () => disableRequests++,
            requestExit: () => exitRequests++,
            isRuntimeEnabled: () => true);

        ApplicationSelfExitDecision decision = sentinel.EvaluateOnce();

        Assert.True(decision.ShouldExit);
        Assert.Equal(1, disableRequests);
        Assert.Equal(1, exitRequests);
        Assert.Contains("inspection failed", sentinel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingProcessSnapshotReader : IProcessSnapshotReader
    {
        public IReadOnlyList<ProcessSnapshot> Processes { get; init; } = [];

        public Exception? ReadException { get; init; }

        public int ReadRequests { get; private set; }

        public IReadOnlyList<ProcessSnapshot> ReadProcesses()
        {
            ReadRequests++;
            if (ReadException is not null)
            {
                throw ReadException;
            }

            return Processes;
        }
    }
}
