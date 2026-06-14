namespace MouseShenanigans.Tray;

public interface ILocalControlWebApplicationFactory
{
    ILocalControlWebApplication Create(LocalControlOptions options, LocalControlEndpointHandler handler);
}
