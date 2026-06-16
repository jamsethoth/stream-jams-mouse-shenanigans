namespace MouseShenanigans.Windows;

public enum ApplicationSafetyDenialReason
{
    None,
    AllowlistEmpty,
    TargetIdentityUnavailable,
    TargetNotAllowed,
    ProtectedGameDenyRule,
}

public sealed record ApplicationSafetyDecision
{
    private ApplicationSafetyDecision(
        bool allowed,
        ApplicationSafetyDenialReason denialReason,
        ApplicationIdentity? targetIdentity,
        ApplicationSafetyEntry? matchedAllowlistEntry,
        ApplicationSafetyEntry? matchedProtectedDenyRule,
        ApplicationSafetyClassification? classification,
        string message)
    {
        Allowed = allowed;
        DenialReason = denialReason;
        TargetIdentity = targetIdentity;
        MatchedAllowlistEntry = matchedAllowlistEntry;
        MatchedProtectedDenyRule = matchedProtectedDenyRule;
        Classification = classification;
        Message = message;
    }

    public bool Allowed { get; }

    public ApplicationSafetyDenialReason DenialReason { get; }

    public ApplicationIdentity? TargetIdentity { get; }

    public ApplicationSafetyEntry? MatchedAllowlistEntry { get; }

    public ApplicationSafetyEntry? MatchedProtectedDenyRule { get; }

    public ApplicationSafetyClassification? Classification { get; }

    public string Message { get; }

    public static ApplicationSafetyDecision Allow(
        ApplicationIdentity targetIdentity,
        ApplicationSafetyEntry matchedAllowlistEntry,
        ApplicationSafetyClassification? classification = null)
    {
        return new ApplicationSafetyDecision(
            allowed: true,
            ApplicationSafetyDenialReason.None,
            targetIdentity,
            matchedAllowlistEntry,
            matchedProtectedDenyRule: null,
            classification,
            $"Application safety allowed target '{targetIdentity.DisplayName}' by '{matchedAllowlistEntry.DisplayName}'.");
    }

    public static ApplicationSafetyDecision AllowNonGameUtility(
        ApplicationIdentity targetIdentity,
        ApplicationSafetyClassification? classification = null)
    {
        return new ApplicationSafetyDecision(
            allowed: true,
            ApplicationSafetyDenialReason.None,
            targetIdentity,
            matchedAllowlistEntry: null,
            matchedProtectedDenyRule: null,
            classification,
            $"Application safety allowed non-game utility target '{targetIdentity.DisplayName}'.");
    }

    public static ApplicationSafetyDecision Deny(
        ApplicationSafetyDenialReason reason,
        ApplicationIdentity? targetIdentity,
        string message,
        ApplicationSafetyEntry? matchedProtectedDenyRule = null,
        ApplicationSafetyClassification? classification = null)
    {
        return new ApplicationSafetyDecision(
            allowed: false,
            reason,
            targetIdentity,
            matchedAllowlistEntry: null,
            matchedProtectedDenyRule,
            classification,
            message);
    }
}
