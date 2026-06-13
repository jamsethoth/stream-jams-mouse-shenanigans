using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed record RuntimeConfiguration
{
    private RuntimeConfiguration(
        RuntimeTargetSelector targetSelector,
        string activeProfileName,
        bool cursorLockEnabled,
        RemappingProfileSet profiles)
    {
        TargetSelector = targetSelector;
        ActiveProfileName = activeProfileName;
        CursorLockEnabled = cursorLockEnabled;
        Profiles = profiles;
    }

    public RuntimeTargetSelector TargetSelector { get; }

    public string ActiveProfileName { get; }

    public bool CursorLockEnabled { get; }

    public RemappingProfileSet Profiles { get; }

    public RemappingProfile ActiveProfile => Profiles.GetRequired(ActiveProfileName);

    public IReadOnlyList<string> ProfileNames => Profiles.Profiles.Select(profile => profile.Name).ToArray();

    public string TargetDisplayName
    {
        get
        {
            if (TargetSelector.ProcessName is { } processName)
            {
                return processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                    ? processName
                    : $"{processName}.exe";
            }

            return $"title contains '{TargetSelector.WindowTitleContains}'";
        }
    }

    public static RuntimeConfiguration Create(
        RuntimeTargetSelector targetSelector,
        string activeProfileName,
        bool cursorLockEnabled,
        RemappingProfileSet profiles)
    {
        ArgumentNullException.ThrowIfNull(targetSelector);
        ArgumentNullException.ThrowIfNull(profiles);

        if (string.IsNullOrWhiteSpace(activeProfileName))
        {
            throw new ArgumentException("Active profile name must not be empty.", nameof(activeProfileName));
        }

        string normalizedActiveProfileName = activeProfileName.Trim();
        RemappingProfileSet mergedProfiles = MergeBuiltInProfiles(profiles.Profiles);
        mergedProfiles.GetRequired(normalizedActiveProfileName);
        return new RuntimeConfiguration(
            targetSelector,
            normalizedActiveProfileName,
            cursorLockEnabled,
            mergedProfiles);
    }

    public static RuntimeConfiguration CreateFromConfiguredProfiles(
        RuntimeTargetSelector targetSelector,
        string activeProfileName,
        bool cursorLockEnabled,
        IEnumerable<RemappingProfile> configuredProfiles)
    {
        ArgumentNullException.ThrowIfNull(configuredProfiles);

        RemappingProfileSet profiles = MergeBuiltInProfiles(configuredProfiles);
        return Create(targetSelector, activeProfileName, cursorLockEnabled, profiles);
    }

    public RuntimeConfiguration WithActiveProfile(string activeProfileName)
    {
        return Create(TargetSelector, activeProfileName, CursorLockEnabled, Profiles);
    }

    public RuntimeConfiguration WithTargetSelector(RuntimeTargetSelector targetSelector)
    {
        return Create(targetSelector, ActiveProfileName, CursorLockEnabled, Profiles);
    }

    public RuntimeRemappingOptions CreateRuntimeOptions()
    {
        return new RuntimeRemappingOptions(
            TargetSelector,
            ActiveProfile,
            cursorLockEnabled: CursorLockEnabled);
    }

    public static bool IsBuiltInProfileName(string profileName)
    {
        return BuiltInRemappingProfiles.All.Any(
            profile => string.Equals(profile.Name, profileName, StringComparison.OrdinalIgnoreCase));
    }

    private static RemappingProfileSet MergeBuiltInProfiles(IEnumerable<RemappingProfile> configuredProfiles)
    {
        HashSet<string> builtInNames = BuiltInRemappingProfiles.All
            .Select(profile => profile.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return RemappingProfileSet.Create(BuiltInRemappingProfiles.All.Concat(
            configuredProfiles.Where(profile => !builtInNames.Contains(profile.Name))));
    }
}
