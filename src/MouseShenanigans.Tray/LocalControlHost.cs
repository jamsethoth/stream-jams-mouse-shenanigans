using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class LocalControlHost : IDisposable
{
    private readonly LocalControlOptions options;
    private readonly LocalControlEndpointHandler handler;
    private readonly ILocalControlWebApplicationFactory factory;
    private readonly IDiagnosticRecorder diagnosticRecorder;
    private ILocalControlWebApplication? application;
    private bool disposed;

    public LocalControlHost(
        LocalControlOptions options,
        LocalControlEndpointHandler handler,
        ILocalControlWebApplicationFactory factory,
        IDiagnosticRecorder? diagnosticRecorder = null,
        string? startupValidationFailureMessage = null)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.diagnosticRecorder = diagnosticRecorder ?? NullDiagnosticRecorder.Instance;

        if (!string.IsNullOrWhiteSpace(startupValidationFailureMessage))
        {
            Status = LocalControlHostStatus.Failed(startupValidationFailureMessage);
            this.diagnosticRecorder.Record(
                DiagnosticEventTypes.LocalControlStartupFailed,
                Status.Message ?? startupValidationFailureMessage);
        }
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
            diagnosticRecorder.Record(
                DiagnosticEventTypes.LocalControlStarted,
                $"Local control started at {options.UrlText}.");
        }
        catch (Exception ex)
        {
            application?.Dispose();
            application = null;
            Status = LocalControlHostStatus.Failed(ex.Message);
            diagnosticRecorder.Record(
                DiagnosticEventTypes.LocalControlStartupFailed,
                Status.Message ?? ex.Message);
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        try
        {
            application?.StopAcceptingRequests();
        }
        catch (Exception)
        {
        }
        finally
        {
            try
            {
                application?.Dispose();
            }
            catch (Exception)
            {
            }

            application = null;
            Status = LocalControlHostStatus.Stopped;
        }
    }
}
