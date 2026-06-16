namespace MouseShenanigans.Windows;

public interface IDiagnosticRecorder
{
    void Record(string type, string message, DiagnosticCapturedIdentity? capturedIdentity = null);

    IReadOnlyList<DiagnosticEvent> Snapshot();
}
