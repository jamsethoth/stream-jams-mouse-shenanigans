using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public static class RuntimeProofOfConceptDefaults
{
    public const string TargetProcessName = "Streamer.bot.exe";

    public const string ActiveProfileName = "horizontal-inversion";

    public static RemappingProfile HorizontalInversionProfile { get; } = new(
        ActiveProfileName,
        left: new MovementVector(1, 0),
        right: new MovementVector(-1, 0),
        up: new MovementVector(0, -1),
        down: new MovementVector(0, 1));

    public static RuntimeRemappingOptions CreateOptions()
    {
        return CreateConfiguration().CreateRuntimeOptions();
    }

    public static RuntimeConfiguration CreateConfiguration()
    {
        return RuntimeConfiguration.CreateFromConfiguredProfiles(
            RuntimeTargetSelector.ForProcessName(TargetProcessName),
            ActiveProfileName,
            cursorLockEnabled: true,
            [HorizontalInversionProfile]);
    }
}
