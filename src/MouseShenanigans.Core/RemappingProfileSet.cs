namespace MouseShenanigans.Core;

public sealed class RemappingProfileSet
{
    private readonly Dictionary<string, RemappingProfile> profilesByName;

    private RemappingProfileSet(IReadOnlyList<RemappingProfile> profiles)
    {
        Profiles = profiles;
        profilesByName = profiles.ToDictionary(
            profile => profile.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<RemappingProfile> Profiles { get; }

    public static RemappingProfileSet Create(IEnumerable<RemappingProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        RemappingProfile[] profileArray = profiles.ToArray();
        if (profileArray.Length == 0)
        {
            throw new ArgumentException("Profile collection must contain at least one profile.", nameof(profiles));
        }

        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (RemappingProfile profile in profileArray)
        {
            if (!names.Add(profile.Name))
            {
                throw new ArgumentException(
                    $"Duplicate remapping profile name '{profile.Name}'.",
                    nameof(profiles));
            }
        }

        return new RemappingProfileSet(profileArray);
    }

    public RemappingProfile GetRequired(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Profile name must not be empty.", nameof(name));
        }

        if (profilesByName.TryGetValue(name, out RemappingProfile? profile))
        {
            return profile;
        }

        throw new KeyNotFoundException($"Remapping profile '{name}' was not found.");
    }
}
