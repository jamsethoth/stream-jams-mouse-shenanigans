using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class LocalControlHostTests
{
    [Fact]
    public void DefaultOptionsBindToLoopbackUrl()
    {
        Assert.Equal("http://127.0.0.1:5178", LocalControlOptions.Default.UrlText);
        Assert.True(LocalControlOptions.Default.Url.IsLoopback);
    }

    [Fact]
    public void OptionsRejectNonLoopbackUrl()
    {
        Assert.Throws<ArgumentException>(() => LocalControlOptions.Create("http://0.0.0.0:5178"));
    }

    [Fact]
    public void StartReportsAvailableWhenApplicationStarts()
    {
        var factory = new RecordingApplicationFactory();
        var recorder = new BoundedDiagnosticRecorder();
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory,
            recorder);

        host.Start();

        Assert.Equal(LocalControlHostState.Available, host.Status.State);
        Assert.Equal(LocalControlOptions.Default.UrlText, host.Status.Url);
        Assert.Single(factory.Applications);
        Assert.Equal(["start"], factory.Applications[0].Operations);
        DiagnosticEvent diagnosticEvent = Assert.Single(recorder.Snapshot());
        Assert.Equal(DiagnosticEventTypes.LocalControlStarted, diagnosticEvent.Type);
        Assert.Contains(LocalControlOptions.Default.UrlText, diagnosticEvent.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StartFailureReportsDegradedStatusWithoutThrowing()
    {
        var factory = new RecordingApplicationFactory
        {
            CreateException = new InvalidOperationException("port already in use"),
        };
        var recorder = new BoundedDiagnosticRecorder();
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory,
            recorder);

        host.Start();

        Assert.Equal(LocalControlHostState.Failed, host.Status.State);
        Assert.Contains("port already in use", host.Status.Message, StringComparison.Ordinal);
        DiagnosticEvent diagnosticEvent = Assert.Single(recorder.Snapshot());
        Assert.Equal(DiagnosticEventTypes.LocalControlStartupFailed, diagnosticEvent.Type);
        Assert.Contains("port already in use", diagnosticEvent.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StartupValidationFailureDoesNotBindLocalControl()
    {
        var factory = new RecordingApplicationFactory();
        var recorder = new BoundedDiagnosticRecorder();
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory,
            recorder,
            startupValidationFailureMessage: "Local control URL must be loopback.");

        host.Start();

        Assert.Equal(LocalControlHostState.Failed, host.Status.State);
        Assert.Empty(factory.Applications);
        DiagnosticEvent diagnosticEvent = Assert.Single(recorder.Snapshot());
        Assert.Equal(DiagnosticEventTypes.LocalControlStartupFailed, diagnosticEvent.Type);
    }

    [Fact]
    public void DisposeStopsApplicationBeforeDisposal()
    {
        var factory = new RecordingApplicationFactory();
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory);
        host.Start();

        host.Dispose();

        Assert.Equal(["start", "stop", "dispose"], factory.Applications[0].Operations);
        Assert.Equal(LocalControlHostState.Stopped, host.Status.State);
    }

    [Fact]
    public void DisposeStillMarksStoppedWhenStopFails()
    {
        var factory = new RecordingApplicationFactory
        {
            StopException = new InvalidOperationException("stop hung"),
        };
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory);
        host.Start();

        host.Dispose();

        Assert.Equal(["start", "stop", "dispose"], factory.Applications[0].Operations);
        Assert.Equal(LocalControlHostState.Stopped, host.Status.State);
    }

    [Fact]
    public void DisposeStillMarksStoppedWhenApplicationDisposeFails()
    {
        var factory = new RecordingApplicationFactory
        {
            DisposeException = new InvalidOperationException("dispose failed"),
        };
        using var host = new LocalControlHost(
            LocalControlOptions.Default,
            CreateHandler(),
            factory);
        host.Start();

        host.Dispose();

        Assert.Equal(["start", "stop", "dispose"], factory.Applications[0].Operations);
        Assert.Equal(LocalControlHostState.Stopped, host.Status.State);
    }

    private static LocalControlEndpointHandler CreateHandler()
    {
        var runtime = new RecordingRuntimeController();
        return new LocalControlEndpointHandler(new RuntimeCommandController(runtime));
    }

    private sealed class RecordingApplicationFactory : ILocalControlWebApplicationFactory
    {
        public Exception? CreateException { get; init; }

        public Exception? StopException { get; init; }

        public Exception? DisposeException { get; init; }

        public List<RecordingApplication> Applications { get; } = [];

        public ILocalControlWebApplication Create(LocalControlOptions options, LocalControlEndpointHandler handler)
        {
            if (CreateException is not null)
            {
                throw CreateException;
            }

            var application = new RecordingApplication(StopException, DisposeException);
            Applications.Add(application);
            return application;
        }
    }

    private sealed class RecordingApplication(Exception? stopException, Exception? disposeException)
        : ILocalControlWebApplication
    {
        public List<string> Operations { get; } = [];

        public void Start()
        {
            Operations.Add("start");
        }

        public void StopAcceptingRequests()
        {
            Operations.Add("stop");
            if (stopException is not null)
            {
                throw stopException;
            }
        }

        public void Dispose()
        {
            Operations.Add("dispose");
            if (disposeException is not null)
            {
                throw disposeException;
            }
        }
    }

    private sealed class RecordingRuntimeController : IRuntimeRemappingController
    {
        public RuntimeRemappingStatus Status { get; } = RuntimeRemappingStatus.Disabled;

        public bool IsCursorLockEnabled { get; private set; }

        public void SetCursorLockEnabled(bool enabled)
        {
            IsCursorLockEnabled = enabled;
        }

        public void ApplyOptions(RuntimeRemappingOptions options)
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
            IsCursorLockEnabled = false;
        }

        public void Dispose()
        {
        }
    }
}
