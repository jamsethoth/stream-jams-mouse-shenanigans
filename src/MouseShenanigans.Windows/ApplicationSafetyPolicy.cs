using System.IO.Enumeration;

namespace MouseShenanigans.Windows;

public static class ApplicationSafetyPolicy
{
    public static ApplicationSafetyDecision EvaluateEnable(RuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ApplicationSafetyConfiguration safety = configuration.Safety;
        ApplicationIdentity? targetIdentity = ApplicationIdentity.FromTargetSelector(configuration.TargetSelector);
        if (targetIdentity is null)
        {
            return ApplicationSafetyDecision.Deny(
                ApplicationSafetyDenialReason.TargetIdentityUnavailable,
                targetIdentity: null,
                "Application safety blocked enable: configured target identity is unavailable.");
        }

        ApplicationSafetyClassification classification = Classify(safety, targetIdentity);
        if (classification.MatchedProtectedDenyRule is { } protectedRule)
        {
            return ApplicationSafetyDecision.Deny(
                ApplicationSafetyDenialReason.ProtectedGameDenyRule,
                targetIdentity,
                $"Application safety blocked enable: target '{targetIdentity.DisplayName}' matched protected deny rule '{protectedRule.DisplayName}'.",
                protectedRule,
                classification);
        }

        if (!classification.IsGameCandidate)
        {
            return ApplicationSafetyDecision.AllowNonGameUtility(targetIdentity, classification);
        }

        if (classification.MatchedAllowlistEntry is { } matchedEntry)
        {
            return ApplicationSafetyDecision.Allow(targetIdentity, matchedEntry, classification);
        }

        ApplicationSafetyDenialReason reason = safety.AllowlistedGames.Count == 0
            ? ApplicationSafetyDenialReason.AllowlistEmpty
            : ApplicationSafetyDenialReason.TargetNotAllowed;
        string ruleText = classification.MatchedGameCandidateRule is null
            ? string.Empty
            : $" Matched game rule: {classification.MatchedGameCandidateRule}.";
        return ApplicationSafetyDecision.Deny(
            reason,
            targetIdentity,
            $"Application safety blocked enable: game target '{targetIdentity.DisplayName}' is not allowlisted.{ruleText}",
            classification: classification);
    }

    public static ApplicationSelfExitDecision EvaluateSelfExit(
        ApplicationSafetyConfiguration safety,
        IEnumerable<ProcessSnapshot> runningProcesses,
        bool runtimeEnabled = true)
    {
        ArgumentNullException.ThrowIfNull(safety);
        ArgumentNullException.ThrowIfNull(runningProcesses);

        if (!safety.ShouldInspectProcesses(runtimeEnabled))
        {
            return ApplicationSelfExitDecision.Continue;
        }

        foreach (ProcessSnapshot process in runningProcesses)
        {
            ApplicationSafetyClassification classification = Classify(safety, process.Identity);
            if (!classification.HasReadableIdentity)
            {
                if (runtimeEnabled)
                {
                    return ApplicationSelfExitDecision.Exit(process, null, $"process {process.ProcessId} identity was unreadable");
                }

                continue;
            }

            if (classification.MatchedProtectedDenyRule is { } protectedRule)
            {
                return ApplicationSelfExitDecision.Exit(
                    process,
                    protectedRule,
                    $"running process '{process.Identity!.DisplayName}' matched protected deny rule '{protectedRule.DisplayName}'");
            }

            if (runtimeEnabled && classification is { IsGameCandidate: true, MatchedAllowlistEntry: null })
            {
                string ruleText = classification.MatchedGameCandidateRule is null
                    ? "an unallowlisted game candidate rule"
                    : $"game rule '{classification.MatchedGameCandidateRule}'";
                return ApplicationSelfExitDecision.Exit(
                    process,
                    null,
                    $"running process '{process.Identity!.DisplayName}' matched {ruleText} without an allowlist entry");
            }
        }

        return ApplicationSelfExitDecision.Continue;
    }

    public static ApplicationSafetyClassification Classify(
        ApplicationSafetyConfiguration safety,
        ApplicationIdentity? identity)
    {
        ArgumentNullException.ThrowIfNull(safety);

        if (identity is null)
        {
            return ApplicationSafetyClassification.IdentityUnavailable;
        }

        ApplicationSafetyEntry? protectedRule = FirstMatchingEntry(safety.ProtectedGameDenyRules, identity);
        ApplicationSafetyEntry? allowlistEntry = FirstMatchingEntry(safety.AllowlistedGames, identity);
        string? gameRule = protectedRule?.DisplayName
            ?? allowlistEntry?.DisplayName
            ?? MatchGameProcessPattern(safety.GameProcessPatterns, identity)
            ?? MatchGameLibraryRoot(safety.GameLibraryRoots, identity);

        return gameRule is null
            ? ApplicationSafetyClassification.NonGameUtility
            : new ApplicationSafetyClassification(
                HasReadableIdentity: true,
                IsGameCandidate: true,
                allowlistEntry,
                protectedRule,
                gameRule);
    }

    private static ApplicationSafetyEntry? FirstMatchingEntry(
        IEnumerable<ApplicationSafetyEntry> entries,
        ApplicationIdentity identity)
    {
        return entries.FirstOrDefault(entry =>
            entry.Identity.MatchesTargetIdentity(identity)
            || entry.Identity.Matches(identity)
            || identity.MatchesTargetIdentity(entry.Identity));
    }

    private static string? MatchGameProcessPattern(
        IEnumerable<string> patterns,
        ApplicationIdentity identity)
    {
        string? processName = identity.ProcessName;
        if (processName is null)
        {
            return null;
        }

        string normalizedProcessName = ApplicationIdentity.NormalizeProcessName(processName) ?? processName;
        foreach (string pattern in patterns)
        {
            string normalizedPattern = ApplicationIdentity.NormalizeProcessName(pattern) ?? pattern;
            if (WildcardMatches(normalizedProcessName, normalizedPattern))
            {
                return pattern;
            }
        }

        return null;
    }

    private static string? MatchGameLibraryRoot(
        IEnumerable<string> roots,
        ApplicationIdentity identity)
    {
        if (identity.ExecutablePath is null)
        {
            return null;
        }

        string executablePath = Path.GetFullPath(identity.ExecutablePath);
        foreach (string root in roots)
        {
            if (IsSameOrChildPath(executablePath, root))
            {
                return root;
            }
        }

        return null;
    }

    private static bool IsSameOrChildPath(string path, string root)
    {
        string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return path.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool WildcardMatches(string text, string pattern)
    {
        return FileSystemName.MatchesSimpleExpression(pattern, text, ignoreCase: true);
    }
}
