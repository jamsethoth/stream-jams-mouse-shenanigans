namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class RepositoryPaths
{
    public static string Root { get; } = FindRepositoryRoot(AppContext.BaseDirectory);

    public static string TrayProjectPath =>
        Path.Combine(Root, "src", "MouseShenanigans.Tray", "MouseShenanigans.Tray.csproj");

    public static string TestWindowFixtureProjectPath =>
        Path.Combine(Root, "tests", "MouseShenanigans.TestWindowFixture", "MouseShenanigans.TestWindowFixture.csproj");

    public static string SourceRoot => Path.Combine(Root, "src");

    private static string FindRepositoryRoot(string startDirectory)
    {
        DirectoryInfo? current = new(startDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MouseShenanigans.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not find repository root above '{startDirectory}'.");
    }
}
