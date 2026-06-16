namespace MouseShenanigans.Windows;

public sealed record DiagnosticCapturedIdentity(
    string? ProcessName = null,
    string? WindowTitle = null,
    string? RuleName = null);
