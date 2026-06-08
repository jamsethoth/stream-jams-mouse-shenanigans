using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public static class RuntimeProofOfConceptDefaults
{
    public const string TargetProcessName = "Streamer.bot.exe";

    public static RuntimeRemappingOptions CreateOptions()
    {
        return new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName(TargetProcessName),
            BuiltInRemappingProfiles.HorizontalInversion);
    }
}
