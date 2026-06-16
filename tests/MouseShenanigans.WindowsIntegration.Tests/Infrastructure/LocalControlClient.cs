using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class LocalControlClient : IDisposable
{
    private readonly HttpClient httpClient;

    public LocalControlClient(Uri baseUri)
    {
        httpClient = new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(3),
        };
    }

    public Task<JsonDocument> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return GetJsonAsync("/api/v1/status", cancellationToken);
    }

    public Task<JsonDocument> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        return GetJsonAsync("/api/v1/diagnostics", cancellationToken);
    }

    public Task<JsonDocument> CaptureForegroundTargetAsync(CancellationToken cancellationToken = default)
    {
        return PostJsonAsync("/api/v1/target/capture-foreground", cancellationToken);
    }

    public Task<JsonDocument> EnableRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return PostJsonAsync("/api/v1/runtime/enable", cancellationToken);
    }

    public Task<JsonDocument> DisableRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return PostJsonAsync("/api/v1/runtime/disable", cancellationToken);
    }

    public Task<JsonDocument> EmergencyDisableRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return PostJsonAsync("/api/v1/runtime/emergency-disable", cancellationToken);
    }

    public Task<JsonDocument> ReloadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return PostJsonAsync("/api/v1/config/reload", cancellationToken);
    }

    public Task<JsonDocument> WaitForStatusAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return WaitForJsonAsync(GetStatusAsync, timeout, cancellationToken);
    }

    public Task<JsonDocument> WaitForDiagnosticsAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return WaitForJsonAsync(GetDiagnosticsAsync, timeout, cancellationToken);
    }

    private static async Task<JsonDocument> WaitForJsonAsync(
        Func<CancellationToken, Task<JsonDocument>> getJson,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? lastException = null;
        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await getJson(cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
            {
                lastException = exception;
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }

        throw new TimeoutException(
            $"Local control endpoint did not become ready within {timeout}.",
            lastException);
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadJsonAsync(path, response, cancellationToken);
    }

    private async Task<JsonDocument> PostJsonAsync(string path, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsync(path, content: null, cancellationToken);
        return await ReadJsonAsync(path, response, cancellationToken);
    }

    private static async Task<JsonDocument> ReadJsonAsync(
        string path,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException(
                $"{path} returned {(int)response.StatusCode}: {content}",
                inner: null,
                response.StatusCode);
        }

        return JsonDocument.Parse(content);
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
