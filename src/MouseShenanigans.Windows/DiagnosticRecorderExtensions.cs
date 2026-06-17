namespace MouseShenanigans.Windows;

public static class DiagnosticRecorderExtensions
{
    public static void RecordSafetyBlockedEnable(
        this IDiagnosticRecorder recorder,
        string message,
        DiagnosticCapturedIdentity? capturedIdentity = null)
    {
        Record(recorder, DiagnosticEventTypes.SafetyBlockedEnable, message, capturedIdentity);
    }

    public static void RecordSelfExitRequested(
        this IDiagnosticRecorder recorder,
        string message,
        DiagnosticCapturedIdentity? capturedIdentity = null)
    {
        Record(recorder, DiagnosticEventTypes.SelfExitRequested, message, capturedIdentity);
    }

    private static void Record(
        IDiagnosticRecorder recorder,
        string type,
        string message,
        DiagnosticCapturedIdentity? capturedIdentity)
    {
        ArgumentNullException.ThrowIfNull(recorder);

        recorder.Record(type, message, capturedIdentity);
    }
}
