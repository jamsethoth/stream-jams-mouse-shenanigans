namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class IntegrationTestSettings
{
    public const string RunDesktopTestsEnvironmentVariable =
        "MOUSE_SHENANIGANS_RUN_DESKTOP_TESTS";

    public const string TrayArtifactPathEnvironmentVariable =
        "MOUSE_SHENANIGANS_TRAY_ARTIFACT_PATH";

    public const string TestWindowFixtureArtifactPathEnvironmentVariable =
        "MOUSE_SHENANIGANS_TEST_WINDOW_FIXTURE_ARTIFACT_PATH";

    public static bool RunDesktopTests =>
        string.Equals(
            Environment.GetEnvironmentVariable(RunDesktopTestsEnvironmentVariable),
            "1",
            StringComparison.Ordinal);
}
