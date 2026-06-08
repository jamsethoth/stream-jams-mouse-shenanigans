namespace MouseShenanigans.Windows;

public sealed class RuntimeRemappingCoordinator : IRuntimeRemappingController
{
    private readonly RuntimeRemappingOptions options;
    private readonly IMouseMovementHook hook;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly IMouseMovementInjector injector;
    private readonly RuntimeRemappingDecisionEngine decisionEngine;
    private readonly bool isSupported;
    private bool disposed;

    public RuntimeRemappingCoordinator(
        RuntimeRemappingOptions options,
        IMouseMovementHook hook,
        ITargetWindowReader targetWindowReader,
        IMouseMovementInjector injector)
        : this(
            options,
            hook,
            targetWindowReader,
            injector,
            WindowsRuntime.IsDesktopInputAvailable)
    {
    }

    public RuntimeRemappingCoordinator(
        RuntimeRemappingOptions options,
        IMouseMovementHook hook,
        ITargetWindowReader targetWindowReader,
        IMouseMovementInjector injector,
        bool isSupported)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.hook = hook ?? throw new ArgumentNullException(nameof(hook));
        this.targetWindowReader = targetWindowReader ?? throw new ArgumentNullException(nameof(targetWindowReader));
        this.injector = injector ?? throw new ArgumentNullException(nameof(injector));
        this.isSupported = isSupported;
        decisionEngine = new RuntimeRemappingDecisionEngine(options.ActiveProfile);

        Status = isSupported
            ? RuntimeRemappingStatus.Disabled
            : RuntimeRemappingStatus.Unsupported("Windows desktop input is not available in this session.");
    }

    public RuntimeRemappingStatus Status { get; private set; }

    public void Enable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (!isSupported)
        {
            Status = RuntimeRemappingStatus.Unsupported("Windows desktop input is not available in this session.");
            return;
        }

        if (Status.State == RuntimeRemappingState.Enabled)
        {
            return;
        }

        try
        {
            hook.Start(HandleMovement);
            Status = RuntimeRemappingStatus.Enabled;
        }
        catch (Exception ex)
        {
            TryStopHook();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
        }
    }

    public void Disable()
    {
        if (disposed)
        {
            return;
        }

        if (!isSupported)
        {
            Status = RuntimeRemappingStatus.Unsupported("Windows desktop input is not available in this session.");
            return;
        }

        if (TryStopHook())
        {
            decisionEngine.ResetAccumulator();
            Status = RuntimeRemappingStatus.Disabled;
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        TryStopHook();
        hook.Dispose();
        disposed = true;
    }

    private bool HandleMovement(RuntimeMouseMovement movement)
    {
        if (Status.State != RuntimeRemappingState.Enabled || movement.IsInjected)
        {
            return false;
        }

        try
        {
            TargetWindowSnapshot targetSnapshot = targetWindowReader.ReadSnapshot();
            bool targetMatches = options.TargetSelector.IsMatch(targetSnapshot);
            RuntimeRemappingDecision decision = decisionEngine.Decide(
                movement,
                isEnabled: true,
                targetMatches);

            if (decision.InjectedMovement is { } replacement)
            {
                injector.Inject(replacement);
            }

            return decision.SuppressOriginalMovement;
        }
        catch (Exception ex)
        {
            TryStopHook();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
            return false;
        }
    }

    private bool TryStopHook()
    {
        try
        {
            hook.StopHook();
            return true;
        }
        catch (Exception ex)
        {
            Status = RuntimeRemappingStatus.Failed(ex.Message);
            return false;
        }
    }
}
