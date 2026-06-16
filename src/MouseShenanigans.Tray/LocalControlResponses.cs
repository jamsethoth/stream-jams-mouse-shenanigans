using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed record LocalControlRuntimeSnapshotResponse(
    bool Ok,
    string State,
    bool CursorLockEnabled,
    string? Target,
    string? ActiveProfile,
    IReadOnlyList<string> Profiles,
    string? Message);

public sealed record LocalControlProfilesResponse(
    bool Ok,
    string? ActiveProfile,
    IReadOnlyList<string> Profiles,
    string? Message);

public sealed record LocalControlApplicationIdentityResponse(
    string? ProcessName,
    string? ExecutablePath,
    string? WindowTitleContains,
    string DisplayName);

public sealed record LocalControlForegroundAllowlistCaptureResponse(
    bool Ok,
    string Status,
    string ConfirmationId,
    LocalControlApplicationIdentityResponse CapturedIdentity,
    string? Message);

public sealed record LocalControlErrorResponse(
    bool Ok,
    string Error,
    string Message);

public sealed record LocalControlDiagnosticsResponse(
    bool Ok,
    IReadOnlyList<DiagnosticEvent> Events);

public sealed record LocalControlSelectProfileRequest(string? Name);
