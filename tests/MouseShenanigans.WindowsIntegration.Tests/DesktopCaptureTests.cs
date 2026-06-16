using System.Text.Json;
using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class DesktopCaptureTests(PublishedTrayApplicationFixture fixture)
    : IClassFixture<PublishedTrayApplicationFixture>
{
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(15);

    [DesktopFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.Desktop)]
    public async Task LocalControlForegroundCapturePersistsFixtureTargetWithoutEnablingRuntime()
    {
        using TemporaryDirectory publishDirectory = TemporaryDirectory.Create("fixture-publish");
        PublishedApplication testWindowFixture = PublishedApplication.LocateOrPublishTestWindowFixture(publishDirectory.DirectoryPath);
        await using TrayAppSession tray = StartTraySession();
        await using TestWindowFixtureSession fixtureWindow = await TestWindowFixtureSession.StartAsync(testWindowFixture);
        await tray.Client.WaitForStatusAsync(ReadyTimeout);
        await fixtureWindow.FocusAsync();

        using JsonDocument status = await tray.Client.CaptureForegroundTargetAsync();

        AssertFixtureTargetPersisted(status, await tray.CreateFailureContextAsync());
    }

    [DesktopFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.Desktop)]
    public async Task HotkeyForegroundCapturePersistsFixtureTargetWithoutEnablingRuntime()
    {
        using TemporaryDirectory publishDirectory = TemporaryDirectory.Create("fixture-publish");
        PublishedApplication testWindowFixture = PublishedApplication.LocateOrPublishTestWindowFixture(publishDirectory.DirectoryPath);
        await using TrayAppSession tray = StartTraySession();
        await using TestWindowFixtureSession fixtureWindow = await TestWindowFixtureSession.StartAsync(testWindowFixture);
        await tray.Client.WaitForStatusAsync(ReadyTimeout);
        await fixtureWindow.FocusAsync();

        KeyboardInput.SendForegroundCaptureHotkey();
        using JsonDocument status = await tray.Client.WaitForStatusAsync(ReadyTimeout);

        AssertFixtureTargetPersisted(status, await tray.CreateFailureContextAsync());
    }

    private TrayAppSession StartTraySession()
    {
        return TrayAppSession.Start(fixture.Application ?? throw new InvalidOperationException("Tray app was not published."));
    }

    private static void AssertFixtureTargetPersisted(JsonDocument status, string failureContext)
    {
        JsonElement root = status.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean(), failureContext);
        Assert.Equal("disabled", root.GetProperty("state").GetString());
        Assert.Contains(
            "MouseShenanigans.TestWindowFixture",
            root.GetProperty("target").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
