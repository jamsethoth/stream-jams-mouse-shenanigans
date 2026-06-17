using System.Text.Json;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class BoundedDiagnosticRecorderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void SnapshotKeepsNewestEventsWithinCapacity()
    {
        DateTimeOffset timestamp = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var recorder = new BoundedDiagnosticRecorder(capacity: 2, clock: () => timestamp);

        recorder.Record(DiagnosticEventTypes.LocalControlStarted, "first");
        recorder.Record(DiagnosticEventTypes.ForegroundConfirmationRequested, "second");
        recorder.Record(
            DiagnosticEventTypes.SafetyBlockedEnable,
            "third",
            new DiagnosticCapturedIdentity(ProcessName: "TargetApp", WindowTitle: "Target App", RuleName: "blocked"));

        IReadOnlyList<DiagnosticEvent> events = recorder.Snapshot();

        Assert.Equal(2, events.Count);
        Assert.Equal(DiagnosticEventTypes.ForegroundConfirmationRequested, events[0].Type);
        Assert.Equal(DiagnosticEventTypes.SafetyBlockedEnable, events[1].Type);
        Assert.Equal(timestamp, events[1].Timestamp);
        Assert.Equal("TargetApp", events[1].CapturedIdentity?.ProcessName);

        string json = JsonSerializer.Serialize(events[1], JsonOptions);
        Assert.Contains("\"type\":\"safety-blocked-enable\"", json, StringComparison.Ordinal);
        Assert.Contains("\"capturedIdentity\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordAppendsJsonLinesWhenDiagnosticsPathIsConfigured()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            "MouseShenanigans.Tests",
            Guid.NewGuid().ToString("N"),
            "diagnostics.jsonl");
        DateTimeOffset timestamp = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var recorder = new BoundedDiagnosticRecorder(clock: () => timestamp, jsonLinesPath: path);

        recorder.Record(DiagnosticEventTypes.SelfExitRequested, "shutdown requested");

        string[] lines = File.ReadAllLines(path);
        Assert.Single(lines);
        Assert.Contains("\"type\":\"self-exit-requested\"", lines[0], StringComparison.Ordinal);
        Assert.Contains("\"message\":\"shutdown requested\"", lines[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ExtensionMethodsRecordProductSafetyAndSelfExitEvents()
    {
        var recorder = new BoundedDiagnosticRecorder();
        var identity = new DiagnosticCapturedIdentity(ProcessName: "TargetApp", WindowTitle: "Target App", RuleName: "rule");

        recorder.RecordSafetyBlockedEnable("blocked", identity);
        recorder.RecordSelfExitRequested("self exit", identity);

        Assert.Equal(
            [
                DiagnosticEventTypes.SafetyBlockedEnable,
                DiagnosticEventTypes.SelfExitRequested,
            ],
            recorder.Snapshot().Select(diagnosticEvent => diagnosticEvent.Type));
    }
}
