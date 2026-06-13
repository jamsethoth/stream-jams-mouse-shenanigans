using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public static class RuntimeProofOfConceptDefaults
{
    public const string TargetProcessName = "Streamer.bot.exe";

    public static RuntimeRemappingOptions CreateOptions()
    {
        return CreateConfiguration().CreateRuntimeOptions();
    }

    public static RuntimeConfiguration CreateConfiguration()
    {
        return RuntimeConfiguration.Create(
            RuntimeTargetSelector.ForProcessName(TargetProcessName),
            BuiltInRemappingProfiles.HorizontalInversion.Name,
            cursorLockEnabled: true,
            RemappingProfileSet.Create([BuiltInRemappingProfiles.HorizontalInversion]));
    }
}
