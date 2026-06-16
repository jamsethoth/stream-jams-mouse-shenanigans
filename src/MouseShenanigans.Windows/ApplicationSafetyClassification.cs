namespace MouseShenanigans.Windows;

public sealed record ApplicationSafetyClassification(
    bool HasReadableIdentity,
    bool IsGameCandidate,
    ApplicationSafetyEntry? MatchedAllowlistEntry,
    ApplicationSafetyEntry? MatchedProtectedDenyRule,
    string? MatchedGameCandidateRule)
{
    public static ApplicationSafetyClassification IdentityUnavailable { get; } = new(
        HasReadableIdentity: false,
        IsGameCandidate: true,
        MatchedAllowlistEntry: null,
        MatchedProtectedDenyRule: null,
        MatchedGameCandidateRule: "identity unavailable");

    public static ApplicationSafetyClassification NonGameUtility { get; } = new(
        HasReadableIdentity: true,
        IsGameCandidate: false,
        MatchedAllowlistEntry: null,
        MatchedProtectedDenyRule: null,
        MatchedGameCandidateRule: null);
}
