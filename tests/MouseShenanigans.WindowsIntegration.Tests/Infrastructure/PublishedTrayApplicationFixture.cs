namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

public sealed class PublishedTrayApplicationFixture : IDisposable
{
    private readonly TemporaryDirectory publishDirectory = TemporaryDirectory.Create("publish");

    public PublishedTrayApplicationFixture()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Application = PublishedApplication.LocateOrPublishTray(publishDirectory.DirectoryPath);
    }

    internal PublishedApplication? Application { get; }

    public void Dispose()
    {
        publishDirectory.Dispose();
    }
}
