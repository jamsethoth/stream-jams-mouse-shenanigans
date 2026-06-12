namespace MouseShenanigans.Windows;

public sealed class RuntimeRemappingCoordinator : IRuntimeRemappingController
{
    private readonly IMouseMovementHook hook;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly IMouseMovementInjector injector;
    private readonly RuntimeTargetReentryGate targetReentryGate;
    private readonly bool isSupported;
    private RuntimeRemappingOptions options;
    private RuntimeRemappingDecisionEngine decisionEngine;
    private bool disposed;
    private bool isCursorLockEnabled;

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
            SystemRuntimeClock.Instance,
            WindowsRuntime.IsDesktopInputAvailable)
    {
    }

    public RuntimeRemappingCoordinator(
        RuntimeRemappingOptions options,
        IMouseMovementHook hook,
        ITargetWindowReader targetWindowReader,
        IMouseMovementInjector injector,
        bool isSupported)
        : this(
            options,
            hook,
            targetWindowReader,
            injector,
            SystemRuntimeClock.Instance,
            isSupported)
    {
    }

    public RuntimeRemappingCoordinator(
        RuntimeRemappingOptions options,
        IMouseMovementHook hook,
        ITargetWindowReader targetWindowReader,
        IMouseMovementInjector injector,
        IRuntimeClock clock,
        bool isSupported)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.hook = hook ?? throw new ArgumentNullException(nameof(hook));
        this.targetWindowReader = targetWindowReader ?? throw new ArgumentNullException(nameof(targetWindowReader));
        this.injector = injector ?? throw new ArgumentNullException(nameof(injector));
        this.isSupported = isSupported;
        isCursorLockEnabled = options.CursorLockEnabled;
        decisionEngine = new RuntimeRemappingDecisionEngine(options.ActiveProfile);
        targetReentryGate = new RuntimeTargetReentryGate(options.TargetReentryGracePeriod, clock);

        Status = isSupported
            ? RuntimeRemappingStatus.Disabled
            : RuntimeRemappingStatus.Unsupported("Windows desktop input is not available in this session.");
    }

    public RuntimeRemappingStatus Status { get; private set; }

    public bool IsCursorLockEnabled => isCursorLockEnabled;

    public void SetCursorLockEnabled(bool enabled)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        isCursorLockEnabled = enabled;
    }

    public void ApplyOptions(RuntimeRemappingOptions options)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;
        isCursorLockEnabled = options.CursorLockEnabled;
        decisionEngine = new RuntimeRemappingDecisionEngine(options.ActiveProfile);
        decisionEngine.ResetAccumulator();
        targetReentryGate.Reset();
    }

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
            targetReentryGate.Reset();
            hook.Start(HandleMovement);
            Status = RuntimeRemappingStatus.Enabled;
        }
        catch (Exception ex)
        {
            TryStopHook();
            targetReentryGate.Reset();
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
            targetReentryGate.Reset();
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
        targetReentryGate.Reset();
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
            RuntimeTargetEligibility eligibility = options.TargetSelector.Evaluate(targetSnapshot);
            RuntimeTargetReadiness readiness = targetReentryGate.Evaluate(eligibility);
            RuntimeRemappingDecision decision = decisionEngine.Decide(
                movement,
                isEnabled: true,
                readiness.IsEligibleForRemapping);

            if (decision.InjectedMovement is { } replacement)
            {
                injector.Inject(replacement);
            }

            return decision.SuppressOriginalMovement;
        }
        catch (Exception ex)
        {
            TryStopHook();
            targetReentryGate.Reset();
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
