namespace MouseShenanigans.Windows;

public sealed class NullDiagnosticRecorder : IDiagnosticRecorder
{
    private NullDiagnosticRecorder()
    {
    }

    public static NullDiagnosticRecorder Instance { get; } = new();

    public void Record(string type, string message, DiagnosticCapturedIdentity? capturedIdentity = null)
    {
    }

    public IReadOnlyList<DiagnosticEvent> Snapshot()
    {
        return [];
    }
}
