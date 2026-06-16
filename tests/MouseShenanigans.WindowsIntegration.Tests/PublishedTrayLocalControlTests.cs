using System.Diagnostics;
using System.Text.Json;
using MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

namespace MouseShenanigans.WindowsIntegration.Tests;

public sealed class PublishedTrayLocalControlTests(PublishedTrayApplicationFixture fixture)
    : IClassFixture<PublishedTrayApplicationFixture>
{
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(15);

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task StatusEndpointBecomesReadyWithIsolatedConfiguration()
    {
        await using TrayAppSession session = StartTraySession();

        using JsonDocument status = await WaitForStatusAsync(session);

        JsonElement root = status.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean(), await session.CreateFailureContextAsync());
        Assert.True(root.TryGetProperty("state", out JsonElement state), await session.CreateFailureContextAsync());
        Assert.False(string.IsNullOrWhiteSpace(state.GetString()), await session.CreateFailureContextAsync());
        Assert.True(File.Exists(session.LaunchOptions.ConfigurationPath), await session.CreateFailureContextAsync());
        Assert.StartsWith(
            session.LaunchOptions.RootDirectory,
            session.LaunchOptions.ConfigurationPath,
            StringComparison.OrdinalIgnoreCase);
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task DiagnosticsEndpointBecomesReadyWithStableResponseShape()
    {
        await using TrayAppSession session = StartTraySession();
        await WaitForStatusAsync(session);

        using JsonDocument diagnostics = await session.Client.WaitForDiagnosticsAsync(ReadyTimeout);

        JsonElement root = diagnostics.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean(), await session.CreateFailureContextAsync());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("events").ValueKind);
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task EmptyGameAllowlistDeniesEnableThroughLocalControl()
    {
        await using TrayAppSession session = StartTraySession(CreateConfigurationJson(
            "TargetGame",
            """
            {
              "allowlistedGames": [],
              "protectedGameDenyRules": [],
              "gameLibraryRoots": [],
              "gameProcessPatterns": [ "TargetGame" ]
            }
            """));
        await WaitForStatusAsync(session);

        using JsonDocument response = await session.Client.EnableRuntimeAsync();
        using JsonDocument diagnostics = await session.Client.GetDiagnosticsAsync();

        Assert.Equal("disabled", response.RootElement.GetProperty("state").GetString());
        Assert.Contains(
            "not allowlisted",
            response.RootElement.GetProperty("message").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertDiagnostic(
            diagnostics,
            "safety-blocked-enable",
            expectedProcessName: "TargetGame",
            expectedRuleName: "TargetGame");
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task ProtectedDenyRuleTakesPrecedenceThroughLocalControl()
    {
        await using TrayAppSession session = StartTraySession(CreateConfigurationJson(
            "TargetGame",
            """
            {
              "allowlistedGames": [
                { "label": "User fixture", "processName": "TargetGame" }
              ],
              "protectedGameDenyRules": [
                { "label": "Protected fixture", "processName": "TargetGame" }
              ],
              "gameLibraryRoots": [],
              "gameProcessPatterns": [ "TargetGame" ]
            }
            """));
        await WaitForStatusAsync(session);

        using JsonDocument response = await session.Client.EnableRuntimeAsync();
        using JsonDocument diagnostics = await session.Client.GetDiagnosticsAsync();

        Assert.Equal("disabled", response.RootElement.GetProperty("state").GetString());
        Assert.Contains(
            "Protected fixture",
            response.RootElement.GetProperty("message").GetString(),
            StringComparison.Ordinal);
        AssertDiagnostic(
            diagnostics,
            "safety-blocked-enable",
            expectedProcessName: "TargetGame",
            expectedRuleName: "Protected fixture");
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task ProtectedProcessSelfExitLeavesMatchedProcessRunning()
    {
        using Process protectedProcess = StartProtectedProcess();
        try
        {
            await using TrayAppSession session = StartTraySession(CreateConfigurationJson(
                "Streamer.bot",
                $$"""
                {
                  "allowlistedGames": [],
                  "protectedGameDenyRules": [
                    { "label": "Protected command fixture", "processName": "{{protectedProcess.ProcessName}}" }
                  ],
                  "gameLibraryRoots": [],
                  "gameProcessPatterns": []
                }
                """));

            await session.WaitForExitAsync(ReadyTimeout);

            Assert.False(protectedProcess.HasExited, await session.CreateFailureContextAsync());
            string diagnostics = File.Exists(session.LaunchOptions.DiagnosticsPath)
                ? File.ReadAllText(session.LaunchOptions.DiagnosticsPath)
                : string.Empty;
            Assert.Contains("self-exit-requested", diagnostics, StringComparison.Ordinal);
            Assert.Contains("Protected command fixture", diagnostics, StringComparison.Ordinal);
        }
        finally
        {
            StopProcess(protectedProcess);
        }
    }

    [WindowsIntegrationFact]
    [Trait("Category", IntegrationTestCategories.WindowsIntegration)]
    [Trait("Category", IntegrationTestCategories.NonDesktop)]
    public async Task FailureContextIncludesProcessOutputDiagnosticsAndConfigPath()
    {
        await using TrayAppSession session = StartTraySession();
        await WaitForStatusAsync(session);

        string context = await session.CreateFailureContextAsync();

        Assert.Contains("STDOUT:", context, StringComparison.Ordinal);
        Assert.Contains("STDERR:", context, StringComparison.Ordinal);
        Assert.Contains("Diagnostics endpoint:", context, StringComparison.Ordinal);
        Assert.Contains(session.LaunchOptions.ConfigurationPath, context, StringComparison.Ordinal);
    }

    private TrayAppSession StartTraySession(string? configurationJson = null)
    {
        return TrayAppSession.Start(
            fixture.Application ?? throw new InvalidOperationException("Tray app was not published."),
            configurationJson);
    }

    private static async Task<JsonDocument> WaitForStatusAsync(TrayAppSession session)
    {
        try
        {
            return await session.Client.WaitForStatusAsync(ReadyTimeout);
        }
        catch (Exception exception) when (exception is HttpRequestException or TimeoutException or TaskCanceledException)
        {
            throw new InvalidOperationException(await session.CreateFailureContextAsync(), exception);
        }
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

    private static void AssertDiagnostic(
        JsonDocument diagnostics,
        string expectedType,
        string expectedProcessName,
        string expectedRuleName)
    {
        foreach (JsonElement diagnosticEvent in diagnostics.RootElement.GetProperty("events").EnumerateArray())
        {
            if (diagnosticEvent.GetProperty("type").GetString() != expectedType)
            {
                continue;
            }

            JsonElement capturedIdentity = diagnosticEvent.GetProperty("capturedIdentity");
            if (capturedIdentity.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            if (capturedIdentity.GetProperty("processName").GetString() == expectedProcessName
                && capturedIdentity.GetProperty("ruleName").GetString()?.Contains(
                    expectedRuleName,
                    StringComparison.Ordinal) == true)
            {
                return;
            }
        }

        throw new InvalidOperationException(
            $"Expected diagnostic '{expectedType}' for '{expectedProcessName}' and rule '{expectedRuleName}'.");
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
