namespace MouseShenanigans.Windows;

public sealed record ApplicationSafetyConfiguration
{
    public ApplicationSafetyConfiguration(
        IEnumerable<ApplicationSafetyEntry>? allowedApplications = null,
        IEnumerable<ApplicationSafetyEntry>? selfExitApplications = null,
        IEnumerable<ApplicationSafetyEntry>? protectedGameDenyRules = null,
        IEnumerable<string>? gameLibraryRoots = null,
        IEnumerable<string>? gameProcessPatterns = null)
    {
        AllowlistedGames = (allowedApplications ?? []).ToArray();
        ProtectedGameDenyRules = (protectedGameDenyRules ?? selfExitApplications ?? []).ToArray();
        GameLibraryRoots = NormalizeRoots(gameLibraryRoots);
        GameProcessPatterns = NormalizePatterns(gameProcessPatterns);
        ValidateEntries(AllowlistedGames, nameof(allowedApplications));
        ValidateEntries(ProtectedGameDenyRules, nameof(protectedGameDenyRules));
    }

    public static ApplicationSafetyConfiguration Empty { get; } = new();

    public IReadOnlyList<ApplicationSafetyEntry> AllowlistedGames { get; }

    public IReadOnlyList<ApplicationSafetyEntry> ProtectedGameDenyRules { get; }

    public IReadOnlyList<string> GameLibraryRoots { get; }

    public IReadOnlyList<string> GameProcessPatterns { get; }

    public IReadOnlyList<ApplicationSafetyEntry> AllowedApplications => AllowlistedGames;

    public IReadOnlyList<ApplicationSafetyEntry> SelfExitApplications => ProtectedGameDenyRules;

    public ApplicationSafetyConfiguration WithAllowedApplication(ApplicationIdentity identity, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (HasAllowedApplication(identity))
        {
            return this;
        }

        return new ApplicationSafetyConfiguration(
            allowedApplications: AllowlistedGames.Append(new ApplicationSafetyEntry(identity, label)),
            protectedGameDenyRules: ProtectedGameDenyRules,
            gameLibraryRoots: GameLibraryRoots,
            gameProcessPatterns: GameProcessPatterns);
    }

    public bool HasAllowedApplication(ApplicationIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return AllowlistedGames.Any(entry =>
            entry.Identity.IsExactSameIdentity(identity)
            || entry.Identity.MatchesTargetIdentity(identity)
            || identity.MatchesTargetIdentity(entry.Identity));
    }

    public bool ShouldInspectProcesses(bool runtimeEnabled)
    {
        return ProtectedGameDenyRules.Count > 0
            || (runtimeEnabled && (GameLibraryRoots.Count > 0 || GameProcessPatterns.Count > 0));
    }

    private static void ValidateEntries(
        IReadOnlyList<ApplicationSafetyEntry> entries,
        string parameterName)
    {
        HashSet<string> identities = new(StringComparer.OrdinalIgnoreCase);
        foreach (ApplicationSafetyEntry entry in entries)
        {
            if (!identities.Add(CreateIdentityKey(entry.Identity)))
            {
                throw new ArgumentException(
                    $"Duplicate application safety entry '{entry.Identity.DisplayName}'.",
                    parameterName);
            }
        }
    }

    private static List<string> NormalizeRoots(IEnumerable<string>? roots)
    {
        if (roots is null)
        {
            return [];
        }

        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        List<string> normalizedRoots = [];
        foreach (string root in roots)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException("Game library root must not be empty.", nameof(roots));
            }

            string trimmed = root.Trim();
            if (trimmed.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException($"Game library root '{trimmed}' contains invalid path characters.", nameof(roots));
            }

            if (!Path.IsPathFullyQualified(trimmed))
            {
                throw new ArgumentException($"Game library root '{trimmed}' must be fully qualified.", nameof(roots));
            }

            string normalized = Path.GetFullPath(trimmed);
            if (!seen.Add(normalized))
            {
                throw new ArgumentException($"Duplicate game library root '{normalized}'.", nameof(roots));
            }

            normalizedRoots.Add(normalized);
        }

        return normalizedRoots;
    }

    private static List<string> NormalizePatterns(IEnumerable<string>? patterns)
    {
        if (patterns is null)
        {
            return [];
        }

        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        List<string> normalizedPatterns = [];
        foreach (string pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Game process pattern must not be empty.", nameof(patterns));
            }

            string normalized = pattern.Trim();
            if (!seen.Add(normalized))
            {
                throw new ArgumentException($"Duplicate game process pattern '{normalized}'.", nameof(patterns));
            }

            normalizedPatterns.Add(normalized);
        }

        return normalizedPatterns;
    }

    private static string CreateIdentityKey(ApplicationIdentity identity)
    {
        return string.Join(
            "\u001F",
            identity.ProcessName ?? string.Empty,
            identity.ExecutablePath ?? string.Empty,
            identity.WindowTitleContains ?? string.Empty);
    }
}
