using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class KestrelLocalControlWebApplicationFactory : ILocalControlWebApplicationFactory
{
    public ILocalControlWebApplication Create(LocalControlOptions options, LocalControlEndpointHandler handler)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(handler);

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls(options.UrlText);

        WebApplication app = builder.Build();
        MapRoutes(app, handler);
        return new KestrelLocalControlWebApplication(app);
    }

    private static void MapRoutes(IEndpointRouteBuilder app, LocalControlEndpointHandler handler)
    {
        app.MapGet("/api/v1/status", () => ToHttpResult(handler.GetStatus()));
        app.MapPost("/api/v1/runtime/enable", () => ToHttpResult(handler.Execute(RuntimeCommand.EnableRuntime)));
        app.MapPost("/api/v1/runtime/disable", () => ToHttpResult(handler.Execute(RuntimeCommand.DisableRuntime)));
        app.MapPost("/api/v1/runtime/toggle", () => ToHttpResult(handler.Execute(RuntimeCommand.ToggleRuntime)));
        app.MapPost("/api/v1/runtime/emergency-disable", () => ToHttpResult(handler.Execute(RuntimeCommand.EmergencyDisable)));
        app.MapGet("/api/v1/profiles", () => ToHttpResult(handler.GetProfiles()));
        app.MapPost("/api/v1/profiles/select", async (HttpRequest request) =>
        {
            LocalControlEndpointResult result;
            try
            {
                LocalControlSelectProfileRequest? body =
                    await request.ReadFromJsonAsync<LocalControlSelectProfileRequest>();
                result = handler.SelectProfile(body);
            }
            catch (JsonException ex)
            {
                result = LocalControlEndpointResult.BadRequest(new LocalControlErrorResponse(
                    Ok: false,
                    Error: LocalControlErrorCodes.InvalidJson,
                    Message: ex.Message));
            }

            return ToHttpResult(result);
        });
        app.MapPost("/api/v1/config/reload", () => ToHttpResult(handler.ReloadConfiguration()));
    }

    private static IResult ToHttpResult(LocalControlEndpointResult result)
    {
        return Results.Json(result.Body, statusCode: result.StatusCode);
    }

    private sealed class KestrelLocalControlWebApplication(WebApplication app) : ILocalControlWebApplication
    {
        public void Start()
        {
            app.StartAsync().GetAwaiter().GetResult();
        }

        public void StopAcceptingRequests()
        {
            app.StopAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
