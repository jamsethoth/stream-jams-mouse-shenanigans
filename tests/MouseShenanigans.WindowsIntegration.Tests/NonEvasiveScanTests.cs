using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class NonEvasiveScanTests(PublishedTrayApplicationFixture fixture)
    : IClassFixture<PublishedTrayApplicationFixture>
{
    private static readonly string[] ForbiddenSourceMarkers =
    [
        "SetWindowsHookEx",
        "WH_MOUSE_LL",
        "CreateRemoteThread",
        "WriteProcessMemory",
        "ReadProcessMemory",
        "VirtualAllocEx",
        "NtCreateThreadEx",
        "anti-cheat",
        "stealth",
        "concealment",
        "kernel driver",
        "service installer",
        "overlay injector",
    ];

    private static readonly string[] ForbiddenArtifactExtensions =
    [
        ".sys",
        ".inf",
        ".msi",
    ];

    private static readonly string[] ForbiddenArtifactNameParts =
    [
        "driver",
        "inject",
        "overlay",
        "elevated",
    ];

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonEvasiveScan)]
    public void SourceDoesNotContainForbiddenInvasiveOrEvasiveMarkers()
    {
        string[] findings = Directory
            .EnumerateFiles(RepositoryPaths.SourceRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(ScanFile)
            .ToArray();

        Assert.Empty(findings);
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonEvasiveScan)]
    public void PublishOutputDoesNotContainForbiddenArtifacts()
    {
        PublishedApplication application =
            fixture.Application ?? throw new InvalidOperationException("Tray app was not published.");
        string[] findings = Directory
            .EnumerateFiles(application.PublishDirectory, "*", SearchOption.AllDirectories)
            .Select(ScanArtifact)
            .Where(finding => finding is not null)
            .Select(finding => finding!)
            .ToArray();

        Assert.Empty(findings);
    }

    private static IEnumerable<string> ScanFile(string path)
    {
        int lineNumber = 0;
        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            foreach (string marker in ForbiddenSourceMarkers)
            {
                if (line.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"{Path.GetRelativePath(RepositoryPaths.Root, path)}:{lineNumber}: contains forbidden marker '{marker}'";
                }
            }
        }
    }

    private static string? ScanArtifact(string path)
    {
        string fileName = Path.GetFileName(path);
        string extension = Path.GetExtension(path);
        if (ForbiddenArtifactExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return $"{Path.GetRelativePath(RepositoryPaths.Root, path)}: forbidden artifact extension '{extension}'";
        }

        string? forbiddenNamePart = ForbiddenArtifactNameParts.FirstOrDefault(
            part => fileName.Contains(part, StringComparison.OrdinalIgnoreCase));
        return forbiddenNamePart is null
            ? null
            : $"{Path.GetRelativePath(RepositoryPaths.Root, path)}: forbidden artifact name marker '{forbiddenNamePart}'";
    }
}
