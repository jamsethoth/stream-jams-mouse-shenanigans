namespace MouseShenanigans.Tray;

public interface ILocalControlWebApplication : IDisposable
{
    void Start();

    void StopAcceptingRequests();
}
