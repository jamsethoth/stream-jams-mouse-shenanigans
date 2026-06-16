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

    private TrayAppSession StartTraySession()
    {
        return TrayAppSession.Start(fixture.Application ?? throw new InvalidOperationException("Tray app was not published."));
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
}
