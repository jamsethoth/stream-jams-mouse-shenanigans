using System.Text.Json;

namespace MouseShenanigans.Core;

public static class RemappingProfileJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static RemappingProfileSet Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException("Profile JSON document must not be empty.");
        }

        try
        {
            ProfileDocumentDto? document = JsonSerializer.Deserialize<ProfileDocumentDto>(json, JsonOptions);
            if (document?.Profiles is not { Count: > 0 })
            {
                throw new InvalidDataException("Profile JSON document must contain at least one profile.");
            }

            RemappingProfile[] profiles = document.Profiles
                .Select(ToProfile)
                .ToArray();

            return RemappingProfileSet.Create(profiles);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Profile JSON document is malformed.", exception);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException("Profile JSON document is invalid.", exception);
        }
    }

    private static RemappingProfile ToProfile(ProfileDto profile)
    {
        if (profile is null)
        {
            throw new InvalidDataException("Profile entry must not be null.");
        }

        return new RemappingProfile(
            profile.Name ?? string.Empty,
            left: ToVector(profile.Left, "left"),
            right: ToVector(profile.Right, "right"),
            up: ToVector(profile.Up, "up"),
            down: ToVector(profile.Down, "down"));
    }

    private static MovementVector ToVector(VectorDto? vector, string direction)
    {
        if (vector is null)
        {
            throw new InvalidDataException($"Profile is missing the {direction} mapping.");
        }

        if (!vector.X.HasValue)
        {
            throw new InvalidDataException($"Profile {direction} mapping is missing x.");
        }

        if (!vector.Y.HasValue)
        {
            throw new InvalidDataException($"Profile {direction} mapping is missing y.");
        }

        return new MovementVector(vector.X.Value, vector.Y.Value);
    }

    private sealed class ProfileDocumentDto
    {
        public IReadOnlyList<ProfileDto>? Profiles { get; init; }
    }

    private sealed class ProfileDto
    {
        public string? Name { get; init; }

        public VectorDto? Left { get; init; }

        public VectorDto? Right { get; init; }

        public VectorDto? Up { get; init; }

        public VectorDto? Down { get; init; }
    }

    private sealed class VectorDto
    {
        public double? X { get; init; }

        public double? Y { get; init; }
    }
}
