using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class DesktopCaptureTests(PublishedTrayApplicationFixture fixture)
    : IClassFixture<PublishedTrayApplicationFixture>
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
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

    [DesktopFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.Desktop)]
    public async Task AllowlistedFixtureEnableSucceedsAndSelfExitLeavesProtectedProcessRunning()
    {
        using TemporaryDirectory publishDirectory = TemporaryDirectory.Create("fixture-publish");
        PublishedApplication testWindowFixture = PublishedApplication.LocateOrPublishTestWindowFixture(publishDirectory.DirectoryPath);
        await using TrayAppSession tray = StartTraySession(CreateConfigurationJson(
            "MouseShenanigans.TestWindowFixture",
            """
            {
              "allowlistedGames": [
                { "label": "Fixture target", "processName": "MouseShenanigans.TestWindowFixture" }
              ],
              "protectedGameDenyRules": [],
              "gameLibraryRoots": [],
              "gameProcessPatterns": [ "MouseShenanigans.TestWindowFixture" ]
            }
            """));
        await using TestWindowFixtureSession fixtureWindow = await TestWindowFixtureSession.StartAsync(testWindowFixture);
        await tray.Client.WaitForStatusAsync(ReadyTimeout);
        await fixtureWindow.FocusAsync();

        using JsonDocument enabledStatus = await tray.Client.EnableRuntimeAsync();

        Assert.Equal("enabled", enabledStatus.RootElement.GetProperty("state").GetString());

        using Process protectedProcess = StartProtectedProcess();
        try
        {
            File.WriteAllText(
                tray.LaunchOptions.ConfigurationPath,
                CreateConfigurationJson(
                    "MouseShenanigans.TestWindowFixture",
                    $$"""
                    {
                      "allowlistedGames": [
                        { "label": "Fixture target", "processName": "MouseShenanigans.TestWindowFixture" }
                      ],
                      "protectedGameDenyRules": [
                        { "label": "Protected command fixture", "processName": "{{protectedProcess.ProcessName}}" }
                      ],
                      "gameLibraryRoots": [],
                      "gameProcessPatterns": [ "MouseShenanigans.TestWindowFixture" ]
                    }
                    """),
                Utf8NoBom);
            using JsonDocument reloadStatus = await tray.Client.ReloadConfigurationAsync();
            Assert.True(reloadStatus.RootElement.GetProperty("ok").GetBoolean());

            await tray.WaitForExitAsync(ReadyTimeout);

            Assert.False(protectedProcess.HasExited, await tray.CreateFailureContextAsync());
        }
        finally
        {
            StopProcess(protectedProcess);
        }
    }

    private TrayAppSession StartTraySession(string? configurationJson = null)
    {
        return TrayAppSession.Start(
            fixture.Application ?? throw new InvalidOperationException("Tray app was not published."),
            configurationJson);
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

    private static string CreateConfigurationJson(string targetProcessName, string safetyJson)
    {
        return $$"""
            {
              "target": { "processName": "{{targetProcessName}}.exe" },
              "activeProfile": "horizontal-inversion",
              "cursorLockEnabled": true,
              "profiles": [
                {
                  "name": "horizontal-inversion",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ],
              "safety": {{safetyJson}}
            }
            """;
    }

    private static Process StartProtectedProcess()
    {
        string commandProcessor = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = commandProcessor,
                Arguments = "/c ping -n 30 127.0.0.1 > nul",
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Protected process fixture did not start.");
        }

        return process;
    }

    private static void StopProcess(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
