using System.Text.Json;
using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public static class RuntimeConfigurationJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static RuntimeConfiguration Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException("Runtime configuration JSON document must not be empty.");
        }

        try
        {
            RuntimeConfigurationDocumentDto? document = JsonSerializer.Deserialize<RuntimeConfigurationDocumentDto>(
                json,
                JsonOptions);

            if (document is null)
            {
                throw new InvalidDataException("Runtime configuration JSON document must contain an object.");
            }

            if (document.Target is null)
            {
                throw new InvalidDataException("Runtime configuration must include a target.");
            }

            if (!document.CursorLockEnabled.HasValue)
            {
                throw new InvalidDataException("Runtime configuration must include cursorLockEnabled.");
            }

            if (document.Profiles is not { Count: > 0 })
            {
                throw new InvalidDataException("Runtime configuration must contain at least one profile.");
            }

            RemappingProfileSet profiles = RemappingProfileSet.Create(document.Profiles.Select(ToProfile));
            RuntimeTargetSelector targetSelector = RuntimeTargetSelector.Create(
                document.Target.ProcessName,
                document.Target.WindowTitleContains);

            return RuntimeConfiguration.Create(
                targetSelector,
                document.ActiveProfile ?? string.Empty,
                document.CursorLockEnabled.Value,
                profiles);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Runtime configuration JSON document is malformed.", exception);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException("Runtime configuration JSON document is invalid.", exception);
        }
        catch (KeyNotFoundException exception)
        {
            throw new InvalidDataException("Runtime configuration active profile was not found.", exception);
        }
    }

    public static string Serialize(RuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var document = new RuntimeConfigurationDocumentDto
        {
            Target = new TargetDto
            {
                ProcessName = configuration.TargetSelector.ProcessName,
                WindowTitleContains = configuration.TargetSelector.WindowTitleContains,
            },
            ActiveProfile = configuration.ActiveProfileName,
            CursorLockEnabled = configuration.CursorLockEnabled,
            Profiles = configuration.Profiles.Profiles.Select(ToDto).ToArray(),
        };

        return JsonSerializer.Serialize(document, JsonOptions);
    }

    private static RemappingProfile ToProfile(ProfileDto profile)
    {
        if (profile is null)
        {
            throw new InvalidDataException("Runtime configuration profile entry must not be null.");
        }

        return new RemappingProfile(
            profile.Name ?? string.Empty,
            ToVector(profile.Left, "left"),
            ToVector(profile.Right, "right"),
            ToVector(profile.Up, "up"),
            ToVector(profile.Down, "down"));
    }

    private static MovementVector ToVector(VectorDto? vector, string direction)
    {
        if (vector is null)
        {
            throw new InvalidDataException($"Runtime configuration profile is missing the {direction} mapping.");
        }

        if (!vector.X.HasValue)
        {
            throw new InvalidDataException($"Runtime configuration profile {direction} mapping is missing x.");
        }

        if (!vector.Y.HasValue)
        {
            throw new InvalidDataException($"Runtime configuration profile {direction} mapping is missing y.");
        }

        return new MovementVector(vector.X.Value, vector.Y.Value);
    }

    private static ProfileDto ToDto(RemappingProfile profile)
    {
        return new ProfileDto
        {
            Name = profile.Name,
            Left = ToDto(profile.Left),
            Right = ToDto(profile.Right),
            Up = ToDto(profile.Up),
            Down = ToDto(profile.Down),
        };
    }

    private static VectorDto ToDto(MovementVector vector)
    {
        return new VectorDto
        {
            X = vector.X,
            Y = vector.Y,
        };
    }

    private sealed class RuntimeConfigurationDocumentDto
    {
        public TargetDto? Target { get; init; }

        public string? ActiveProfile { get; init; }

        public bool? CursorLockEnabled { get; init; }

        public IReadOnlyList<ProfileDto>? Profiles { get; init; }
    }

    private sealed class TargetDto
    {
        public string? ProcessName { get; init; }

        public string? WindowTitleContains { get; init; }
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
