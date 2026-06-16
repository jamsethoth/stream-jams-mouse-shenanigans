namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class PublishedApplication
{
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromMinutes(2);

    private PublishedApplication(string executablePath, string publishDirectory)
    {
        ExecutablePath = executablePath;
        PublishDirectory = publishDirectory;
    }

    public string ExecutablePath { get; }

    public string PublishDirectory { get; }

    public static PublishedApplication LocateOrPublishTray(string publishRoot)
    {
        return LocateOrPublish(
            IntegrationTestSettings.TrayArtifactPathEnvironmentVariable,
            RepositoryPaths.TrayProjectPath,
            "MouseShenanigans.Tray.exe",
            Path.Combine(publishRoot, "tray"));
    }

    public static PublishedApplication LocateOrPublishTestWindowFixture(string publishRoot)
    {
        return LocateOrPublish(
            IntegrationTestSettings.TestWindowFixtureArtifactPathEnvironmentVariable,
            RepositoryPaths.TestWindowFixtureProjectPath,
            "MouseShenanigans.TestWindowFixture.exe",
            Path.Combine(publishRoot, "test-window-fixture"));
    }

    private static PublishedApplication LocateOrPublish(
        string environmentVariable,
        string projectPath,
        string executableName,
        string outputDirectory)
    {
        string? configuredPath = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            string fullPath = Path.GetFullPath(configuredPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"{environmentVariable} points to a missing artifact.",
                    fullPath);
            }

            return new PublishedApplication(fullPath, Path.GetDirectoryName(fullPath)!);
        }

        Directory.CreateDirectory(outputDirectory);
        CommandResult result = CommandRunner.Run(
            "dotnet",
            [
                "publish",
                projectPath,
                "--configuration",
                "Release",
                "--no-restore",
                "--output",
                outputDirectory,
            ],
            PublishTimeout);
        result.EnsureSuccess($"Publishing {Path.GetFileName(projectPath)}");

        string executablePath = Path.Combine(outputDirectory, executableName);
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                $"Published artifact '{executableName}' was not produced.",
                executablePath);
        }

        return new PublishedApplication(executablePath, outputDirectory);
    }
}
