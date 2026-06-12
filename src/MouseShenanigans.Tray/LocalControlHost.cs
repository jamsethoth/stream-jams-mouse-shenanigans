namespace MouseShenanigans.Tray;

public sealed class LocalControlHost : IDisposable
{
    private readonly LocalControlOptions options;
    private readonly LocalControlEndpointHandler handler;
    private readonly ILocalControlWebApplicationFactory factory;
    private ILocalControlWebApplication? application;
    private bool disposed;

    public LocalControlHost(
        LocalControlOptions options,
        LocalControlEndpointHandler handler,
        ILocalControlWebApplicationFactory factory)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public LocalControlHostStatus Status { get; private set; } = LocalControlHostStatus.Stopped;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (application is not null || Status.State == LocalControlHostState.Failed)
        {
            return;
        }

        try
        {
            application = factory.Create(options, handler);
            application.Start();
            Status = LocalControlHostStatus.Available(options.UrlText);
        }
        catch (Exception ex)
        {
            application?.Dispose();
            application = null;
            Status = LocalControlHostStatus.Failed(ex.Message);
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            application?.StopAcceptingRequests();
        }
        finally
        {
            application?.Dispose();
            application = null;
            Status = LocalControlHostStatus.Stopped;
            disposed = true;
        }
    }
}
