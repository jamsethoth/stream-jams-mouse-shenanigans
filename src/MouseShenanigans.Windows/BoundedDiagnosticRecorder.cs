using System.Text;
using System.Text.Json;

namespace MouseShenanigans.Windows;

public sealed class BoundedDiagnosticRecorder : IDiagnosticRecorder
{
    public const int DefaultCapacity = 200;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly object sync = new();
    private readonly Queue<DiagnosticEvent> events = [];
    private readonly Func<DateTimeOffset> clock;
    private readonly string? jsonLinesPath;
    private bool fileWriteFailed;

    public BoundedDiagnosticRecorder(
        int capacity = DefaultCapacity,
        Func<DateTimeOffset>? clock = null,
        string? jsonLinesPath = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Diagnostic history capacity must be positive.");
        }

        Capacity = capacity;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
        this.jsonLinesPath = string.IsNullOrWhiteSpace(jsonLinesPath) ? null : jsonLinesPath;
    }

    public int Capacity { get; }

    public void Record(string type, string message, DiagnosticCapturedIdentity? capturedIdentity = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var diagnosticEvent = new DiagnosticEvent(type, clock(), message, capturedIdentity);

        lock (sync)
        {
            events.Enqueue(diagnosticEvent);
            while (events.Count > Capacity)
            {
                events.Dequeue();
            }

            TryAppendJsonLine(diagnosticEvent);
        }
    }

    public IReadOnlyList<DiagnosticEvent> Snapshot()
    {
        lock (sync)
        {
            return events.ToArray();
        }
    }

    private void TryAppendJsonLine(DiagnosticEvent diagnosticEvent)
    {
        if (jsonLinesPath is null || fileWriteFailed)
        {
            return;
        }

        try
        {
            string? directory = Path.GetDirectoryName(jsonLinesPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(jsonLinesPath, JsonSerializer.Serialize(diagnosticEvent, JsonOptions) + "\n", Utf8NoBom);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            fileWriteFailed = true;
            events.Enqueue(new DiagnosticEvent(
                DiagnosticEventTypes.DiagnosticsWriteFailed,
                clock(),
                $"Diagnostics JSONL output failed: {exception.Message}"));
            while (events.Count > Capacity)
            {
                events.Dequeue();
            }
        }
    }
}
