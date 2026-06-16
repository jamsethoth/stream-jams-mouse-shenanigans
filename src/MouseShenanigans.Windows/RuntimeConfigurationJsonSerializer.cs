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

            RuntimeTargetSelector targetSelector = RuntimeTargetSelector.Create(
                document.Target.ProcessName,
                document.Target.ExecutablePath,
                document.Target.WindowTitleContains);

            return RuntimeConfiguration.CreateFromConfiguredProfiles(
                targetSelector,
                document.ActiveProfile ?? string.Empty,
                document.CursorLockEnabled.Value,
                document.Profiles?.Select(ToProfile) ?? [],
                ToSafetyConfiguration(document.Safety));
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Runtime configuration JSON document is malformed.", exception);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(
                $"Runtime configuration JSON document is invalid: {exception.Message}",
                exception);
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
                ExecutablePath = configuration.TargetSelector.ExecutablePath,
                WindowTitleContains = configuration.TargetSelector.WindowTitleContains,
            },
            ActiveProfile = configuration.ActiveProfileName,
            CursorLockEnabled = configuration.CursorLockEnabled,
            Profiles = configuration.ConfiguredProfiles.Select(ToDto).ToArray(),
            Safety = ToDto(configuration.Safety),
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

    private static ApplicationSafetyConfiguration ToSafetyConfiguration(SafetyDto? safety)
    {
        if (safety is null)
        {
            return ApplicationSafetyConfiguration.Empty;
        }

        return new ApplicationSafetyConfiguration(
            allowedApplications: ToSafetyEntries(
                safety.AllowlistedGames ?? safety.AllowedApplications,
                safety.AllowlistedGames is null ? "allowedApplications" : "allowlistedGames"),
            protectedGameDenyRules: ToSafetyEntries(
                safety.ProtectedGameDenyRules ?? safety.SelfExitApplications,
                safety.ProtectedGameDenyRules is null ? "selfExitApplications" : "protectedGameDenyRules"),
            gameLibraryRoots: safety.GameLibraryRoots,
            gameProcessPatterns: safety.GameProcessPatterns);
    }

    private static ApplicationSafetyEntry[] ToSafetyEntries(
        IReadOnlyList<ApplicationSafetyEntryDto>? entries,
        string collectionName)
    {
        if (entries is null)
        {
            return [];
        }

        return entries
            .Select((entry, index) => ToSafetyEntry(entry, $"{collectionName}[{index}]"))
            .ToArray();
    }

    private static ApplicationSafetyEntry ToSafetyEntry(ApplicationSafetyEntryDto? entry, string name)
    {
        if (entry is null)
        {
            throw new InvalidDataException($"Runtime safety configuration {name} entry must not be null.");
        }

        return new ApplicationSafetyEntry(
            new ApplicationIdentity(entry.ProcessName, entry.ExecutablePath, entry.WindowTitleContains),
            entry.Label);
    }

    private static SafetyDto ToDto(ApplicationSafetyConfiguration safety)
    {
        return new SafetyDto
        {
            AllowlistedGames = safety.AllowlistedGames.Select(ToDto).ToArray(),
            ProtectedGameDenyRules = safety.ProtectedGameDenyRules.Select(ToDto).ToArray(),
            GameLibraryRoots = safety.GameLibraryRoots.ToArray(),
            GameProcessPatterns = safety.GameProcessPatterns.ToArray(),
        };
    }

    private static ApplicationSafetyEntryDto ToDto(ApplicationSafetyEntry entry)
    {
        return new ApplicationSafetyEntryDto
        {
            Label = entry.Label,
            ProcessName = entry.Identity.ProcessName,
            ExecutablePath = entry.Identity.ExecutablePath,
            WindowTitleContains = entry.Identity.WindowTitleContains,
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

        public SafetyDto? Safety { get; init; }
    }

    private sealed class TargetDto
    {
        public string? ProcessName { get; init; }

        public string? ExecutablePath { get; init; }

        public string? WindowTitleContains { get; init; }
    }

    private sealed class SafetyDto
    {
        public IReadOnlyList<ApplicationSafetyEntryDto>? AllowlistedGames { get; init; }

        public IReadOnlyList<ApplicationSafetyEntryDto>? ProtectedGameDenyRules { get; init; }

        public IReadOnlyList<string>? GameLibraryRoots { get; init; }

        public IReadOnlyList<string>? GameProcessPatterns { get; init; }

        public IReadOnlyList<ApplicationSafetyEntryDto>? AllowedApplications { get; init; }

        public IReadOnlyList<ApplicationSafetyEntryDto>? SelfExitApplications { get; init; }
    }

    private sealed class ApplicationSafetyEntryDto
    {
        public string? Label { get; init; }

        public string? ProcessName { get; init; }

        public string? ExecutablePath { get; init; }

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
