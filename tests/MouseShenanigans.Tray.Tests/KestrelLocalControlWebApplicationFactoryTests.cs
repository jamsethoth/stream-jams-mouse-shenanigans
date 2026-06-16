using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class KestrelLocalControlWebApplicationFactoryTests
{
    [Fact]
    public async Task DocumentedEndpointsReturnJsonWithOkField()
    {
        using LocalControlFixture fixture = LocalControlFixture.Start();

        (string Path, Func<Task<HttpResponseMessage>> Send)[] routes =
        [
            ("/api/v1/status", () => fixture.Client.GetAsync("/api/v1/status")),
            ("/api/v1/diagnostics", () => fixture.Client.GetAsync("/api/v1/diagnostics")),
            ("/api/v1/runtime/enable", () => PostAsync(fixture.Client, "/api/v1/runtime/enable")),
            ("/api/v1/runtime/disable", () => PostAsync(fixture.Client, "/api/v1/runtime/disable")),
            ("/api/v1/runtime/toggle", () => PostAsync(fixture.Client, "/api/v1/runtime/toggle")),
            ("/api/v1/runtime/emergency-disable", () => PostAsync(fixture.Client, "/api/v1/runtime/emergency-disable")),
            ("/api/v1/target/capture-foreground", () => PostAsync(fixture.Client, "/api/v1/target/capture-foreground")),
            ("/api/v1/safety/allowed-applications/capture-foreground", () => PostAsync(fixture.Client, "/api/v1/safety/allowed-applications/capture-foreground")),
            ("/api/v1/profiles", () => fixture.Client.GetAsync("/api/v1/profiles")),
            ("/api/v1/profiles/select", () => PostJsonAsync(fixture.Client, "/api/v1/profiles/select", """{ "name": "double-right" }""")),
            ("/api/v1/config/reload", () => PostAsync(fixture.Client, "/api/v1/config/reload")),
        ];

        foreach ((string path, Func<Task<HttpResponseMessage>> send) in routes)
        {
            using HttpResponseMessage response = await send();
            using JsonDocument document = await ReadJsonAsync(response);

            Assert.True(
                response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
                $"{path} returned {(int)response.StatusCode}.");
            Assert.True(document.RootElement.TryGetProperty("ok", out _), $"{path} response did not include ok.");
        }
    }

    [Fact]
    public async Task StatusEndpointReturnsDocumentedSnapshotShape()
    {
        using LocalControlFixture fixture = LocalControlFixture.Start();

        using HttpResponseMessage response = await fixture.Client.GetAsync("/api/v1/status");
        using JsonDocument document = await ReadJsonAsync(response);

        JsonElement root = document.RootElement;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("disabled", root.GetProperty("state").GetString());
        Assert.Equal("TargetApp.exe", root.GetProperty("target").GetString());
        Assert.Equal("horizontal-inversion", root.GetProperty("activeProfile").GetString());
        Assert.Equal(2, root.GetProperty("profiles").GetArrayLength());
        Assert.True(root.TryGetProperty("cursorLockEnabled", out _));
        Assert.True(root.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task SelectProfileRouteRejectsMalformedJsonWithStableError()
    {
        using LocalControlFixture fixture = LocalControlFixture.Start();

        using HttpResponseMessage response = await PostJsonAsync(fixture.Client, "/api/v1/profiles/select", "{");
        using JsonDocument document = await ReadJsonAsync(response);

        JsonElement root = document.RootElement;
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal(LocalControlErrorCodes.InvalidJson, root.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("message").GetString()));
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path)
    {
        return client.PostAsync(path, new ByteArrayContent([]));
    }

    private static Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string path, string json)
    {
        return client.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    private static RuntimeConfiguration CreateConfiguration()
    {
        RemappingProfile doubleRight = new(
            "double-right",
            left: new MovementVector(-1, 0),
            right: new MovementVector(2, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));

        return RuntimeConfiguration.Create(
            RuntimeTargetSelector.ForProcessName("TargetApp.exe"),
            RuntimeProofOfConceptDefaults.ActiveProfileName,
            cursorLockEnabled: false,
            RemappingProfileSet.Create([RuntimeProofOfConceptDefaults.HorizontalInversionProfile, doubleRight]));
    }

    private static string ReserveLoopbackUrl()
    {
        using var listener = new TcpListener(IPAddress.Loopback, port: 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        return $"http://127.0.0.1:{port}";
    }

    private sealed class LocalControlFixture : IDisposable
    {
        private readonly LocalControlHost host;

        private LocalControlFixture(string url)
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(5),
            };

            RuntimeConfiguration configuration = CreateConfiguration();
            var configurationController = new RuntimeConfigurationController(
                new RecordingConfigurationStore(configuration),
                RuntimeProofOfConceptDefaults.CreateConfiguration());
            var runtime = new RecordingRuntimeController();
            var handler = new LocalControlEndpointHandler(new RuntimeCommandController(runtime, configurationController));
            host = new LocalControlHost(
                LocalControlOptions.Create(url),
                handler,
                new KestrelLocalControlWebApplicationFactory());
            host.Start();
        }

        public HttpClient Client { get; }

        public static LocalControlFixture Start()
        {
            return new LocalControlFixture(ReserveLoopbackUrl());
        }

        public void Dispose()
        {
            Client.Dispose();
            host.Dispose();
        }
    }

    private sealed class RecordingConfigurationStore(RuntimeConfiguration configuration) : IRuntimeConfigurationStore
    {
        public string ConfigurationPath => "config.json";

        public RuntimeConfigurationLoadResult LoadOrFallback(RuntimeConfiguration fallbackConfiguration)
        {
            return new RuntimeConfigurationLoadResult(configuration, UsedFallback: false, ErrorMessage: null);
        }

        public RuntimeConfiguration LoadRequired()
        {
            return configuration;
        }

        public void Save(RuntimeConfiguration updatedConfiguration)
        {
            configuration = updatedConfiguration;
        }
    }

    private sealed class RecordingRuntimeController : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; private set; } = RuntimeRemappingStatus.Disabled;

        public bool IsCursorLockEnabled { get; private set; }

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void ApplyOptions(RuntimeRemappingOptions options)
        {
            IsCursorLockEnabled = options.CursorLockEnabled;
        }

        public void Enable()
        {
            Status = RuntimeRemappingStatus.Enabled;
        }

        public void Disable()
        {
            IsCursorLockEnabled = false;
            Status = RuntimeRemappingStatus.Disabled;
        }

        public void Dispose()
        {
        }
    }
}
