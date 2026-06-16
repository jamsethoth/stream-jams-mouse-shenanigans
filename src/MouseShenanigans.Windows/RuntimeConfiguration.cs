using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed record RuntimeConfiguration
{
    private RuntimeConfiguration(
        RuntimeTargetSelector targetSelector,
        string activeProfileName,
        bool cursorLockEnabled,
        RemappingProfileSet profiles,
        IReadOnlyList<RemappingProfile> configuredProfiles,
        ApplicationSafetyConfiguration safety)
    {
        TargetSelector = targetSelector;
        ActiveProfileName = activeProfileName;
        CursorLockEnabled = cursorLockEnabled;
        Profiles = profiles;
        ConfiguredProfiles = configuredProfiles.ToArray();
        Safety = safety;
    }

    public RuntimeTargetSelector TargetSelector { get; }

    public string ActiveProfileName { get; }

    public bool CursorLockEnabled { get; }

    public RemappingProfileSet Profiles { get; }

    public IReadOnlyList<RemappingProfile> ConfiguredProfiles { get; }

    public ApplicationSafetyConfiguration Safety { get; }

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

        return CreateFromConfiguredProfiles(
            targetSelector,
            activeProfileName,
            cursorLockEnabled,
            profiles.Profiles);
    }

    public static RuntimeConfiguration CreateFromConfiguredProfiles(
        RuntimeTargetSelector targetSelector,
        string activeProfileName,
        bool cursorLockEnabled,
        IEnumerable<RemappingProfile> configuredProfiles,
        ApplicationSafetyConfiguration? safety = null)
    {
        ArgumentNullException.ThrowIfNull(targetSelector);
        ArgumentNullException.ThrowIfNull(configuredProfiles);

        if (string.IsNullOrWhiteSpace(activeProfileName))
        {
            throw new ArgumentException("Active profile name must not be empty.", nameof(activeProfileName));
        }

        RemappingProfile[] configuredProfileArray = configuredProfiles.ToArray();
        ValidateConfiguredProfileNames(configuredProfileArray);

        string normalizedActiveProfileName = activeProfileName.Trim();
        RemappingProfileSet profiles = RemappingProfileSet.Create(configuredProfileArray);
        profiles.GetRequired(normalizedActiveProfileName);
        return new RuntimeConfiguration(
            targetSelector,
            normalizedActiveProfileName,
            cursorLockEnabled,
            profiles,
            configuredProfileArray,
            safety ?? ApplicationSafetyConfiguration.Empty);
    }

    public RuntimeConfiguration WithActiveProfile(string activeProfileName)
    {
        return CreateFromConfiguredProfiles(TargetSelector, activeProfileName, CursorLockEnabled, ConfiguredProfiles, Safety);
    }

    public RuntimeConfiguration WithTargetSelector(RuntimeTargetSelector targetSelector)
    {
        return CreateFromConfiguredProfiles(targetSelector, ActiveProfileName, CursorLockEnabled, ConfiguredProfiles, Safety);
    }

    public RuntimeConfiguration WithSafety(ApplicationSafetyConfiguration safety)
    {
        return CreateFromConfiguredProfiles(TargetSelector, ActiveProfileName, CursorLockEnabled, ConfiguredProfiles, safety);
    }

    public RuntimeRemappingOptions CreateRuntimeOptions()
    {
        return new RuntimeRemappingOptions(
            TargetSelector,
            ActiveProfile,
            cursorLockEnabled: CursorLockEnabled);
    }

    private static void ValidateConfiguredProfileNames(IEnumerable<RemappingProfile> configuredProfiles)
    {
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (RemappingProfile profile in configuredProfiles)
        {
            if (!names.Add(profile.Name))
            {
                throw new ArgumentException(
                    $"Duplicate remapping profile name '{profile.Name}'.",
                    nameof(configuredProfiles));
            }
        }
    }
}
