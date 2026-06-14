using Microsoft.AspNetCore.Http;

namespace MouseShenanigans.Tray;

public sealed record LocalControlEndpointResult(int StatusCode, object Body)
{
    public static LocalControlEndpointResult Ok(object body)
    {
        return new LocalControlEndpointResult(StatusCodes.Status200OK, body);
    }

    public static LocalControlEndpointResult BadRequest(LocalControlErrorResponse body)
    {
        return new LocalControlEndpointResult(StatusCodes.Status400BadRequest, body);
    }
}
