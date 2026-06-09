namespace MouseShenanigans.Windows;

public sealed record RuntimeTargetReadiness(
    RuntimeTargetEligibility Eligibility,
    bool IsEligibleForRemapping);
