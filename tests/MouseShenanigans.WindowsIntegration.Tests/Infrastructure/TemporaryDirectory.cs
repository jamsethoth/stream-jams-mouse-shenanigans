namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class TemporaryDirectory : IDisposable
{
    private bool disposed;

    private TemporaryDirectory(string directoryPath)
    {
        DirectoryPath = directoryPath;
        Directory.CreateDirectory(directoryPath);
    }

    public string DirectoryPath { get; }

    public static TemporaryDirectory Create(string prefix)
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "MouseShenanigans.WindowsIntegration",
            $"{prefix}-{Guid.NewGuid():N}");
        return new TemporaryDirectory(directoryPath);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        disposed = true;
    }
}
