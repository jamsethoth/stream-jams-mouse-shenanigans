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

public sealed record LocalControlErrorResponse(
    bool Ok,
    string Error,
    string Message);

public sealed record LocalControlSelectProfileRequest(string? Name);
