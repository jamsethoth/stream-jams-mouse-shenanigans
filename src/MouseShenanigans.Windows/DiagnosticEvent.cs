namespace MouseShenanigans.Windows;

public sealed record DiagnosticEvent(
    string Type,
    DateTimeOffset Timestamp,
    string Message,
    DiagnosticCapturedIdentity? CapturedIdentity = null);
