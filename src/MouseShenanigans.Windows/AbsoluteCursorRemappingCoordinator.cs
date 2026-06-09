namespace MouseShenanigans.Windows;

public sealed class AbsoluteCursorRemappingCoordinator : IRuntimeRemappingController
{
    private readonly RuntimeRemappingOptions options;
    private readonly IRawMouseMovementSource movementSource;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly ICursorPositionController cursorPositionController;
    private readonly ICursorLockController cursorLockController;
    private readonly AbsoluteCursorRemappingDecisionEngine decisionEngine;
    private readonly RuntimeTargetReentryGate targetReentryGate;
    private readonly bool isSupported;
    private bool disposed;
    private bool isCursorLockEnabled;
    private ScreenRectangle? activeCursorLockBounds;

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController)
        : this(
            options,
            movementSource,
            targetWindowReader,
            cursorPositionController,
            new WindowsCursorLockController(),
            SystemRuntimeClock.Instance,
            WindowsRuntime.IsDesktopInputAvailable)
    {
    }

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController,
        bool isSupported)
        : this(
            options,
            movementSource,
            targetWindowReader,
            cursorPositionController,
            new WindowsCursorLockController(),
            SystemRuntimeClock.Instance,
            isSupported)
    {
    }

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController,
        ICursorLockController cursorLockController,
        bool isSupported)
        : this(
            options,
            movementSource,
            targetWindowReader,
            cursorPositionController,
            cursorLockController,
            SystemRuntimeClock.Instance,
            isSupported)
    {
    }

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController,
        ICursorLockController cursorLockController,
        IRuntimeClock clock,
        bool isSupported)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.movementSource = movementSource ?? throw new ArgumentNullException(nameof(movementSource));
        this.targetWindowReader = targetWindowReader ?? throw new ArgumentNullException(nameof(targetWindowReader));
        this.cursorPositionController = cursorPositionController ?? throw new ArgumentNullException(nameof(cursorPositionController));
        this.cursorLockController = cursorLockController ?? throw new ArgumentNullException(nameof(cursorLockController));
        this.isSupported = isSupported;
        isCursorLockEnabled = options.CursorLockEnabled;
        decisionEngine = new AbsoluteCursorRemappingDecisionEngine(
            options.ActiveProfile,
            options.AbsoluteCorrectionScale);
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
        if (!enabled)
        {
            TryReleaseCursorLock();
        }
    }

    public void Enable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (!isSupported)
        {
            TryReleaseCursorLock();
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
            movementSource.Start(HandleMovement);
            Status = RuntimeRemappingStatus.Enabled;
        }
        catch (Exception ex)
        {
            TryStopSource();
            TryReleaseCursorLock();
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
            TryReleaseCursorLock();
            Status = RuntimeRemappingStatus.Unsupported("Windows desktop input is not available in this session.");
            return;
        }

        bool stopped = TryStopSource();
        bool released = TryReleaseCursorLock();
        if (stopped && released)
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

        TryReleaseCursorLock();
        TryStopSource();
        targetReentryGate.Reset();
        movementSource.Dispose();
        disposed = true;
    }

    private void HandleMovement(IntegerMouseDelta rawMovement)
    {
        if (Status.State != RuntimeRemappingState.Enabled || rawMovement.IsZero)
        {
            return;
        }

        try
        {
            TargetWindowSnapshot targetSnapshot = targetWindowReader.ReadSnapshot();
            RuntimeTargetEligibility eligibility = options.TargetSelector.Evaluate(targetSnapshot);
            RuntimeTargetReadiness readiness = targetReentryGate.Evaluate(eligibility);
            UpdateCursorLock(readiness.Eligibility);

            ScreenPoint currentPosition = cursorPositionController.GetPosition();
            AbsoluteCursorRemappingDecision decision = decisionEngine.Decide(
                new RuntimeMouseMovement(rawMovement.Dx, rawMovement.Dy, isInjected: false),
                isEnabled: true,
                readiness.IsEligibleForRemapping,
                currentPosition);

            if (decision.TargetPosition is { } targetPosition)
            {
                cursorPositionController.SetPosition(targetPosition);
            }
        }
        catch (Exception ex)
        {
            TryStopSource();
            TryReleaseCursorLock();
            targetReentryGate.Reset();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
        }
    }

    private void UpdateCursorLock(RuntimeTargetEligibility eligibility)
    {
        if (!isCursorLockEnabled)
        {
            TryReleaseCursorLock();
            return;
        }

        if (eligibility.State is not (RuntimeTargetEligibilityState.InsideBounds or RuntimeTargetEligibilityState.OutsideBounds)
            || eligibility.TargetBounds is not { } bounds)
        {
            TryReleaseCursorLock();
            return;
        }

        if (activeCursorLockBounds == bounds)
        {
            return;
        }

        cursorLockController.LockTo(bounds);
        activeCursorLockBounds = bounds;
    }

    private bool TryReleaseCursorLock()
    {
        if (activeCursorLockBounds is null)
        {
            return true;
        }

        activeCursorLockBounds = null;

        try
        {
            cursorLockController.Release();
            return true;
        }
        catch (Exception ex)
        {
            Status = RuntimeRemappingStatus.Failed(ex.Message);
            return false;
        }
    }

    private bool TryStopSource()
    {
        try
        {
            movementSource.StopObservation();
            return true;
        }
        catch (Exception ex)
        {
            Status = RuntimeRemappingStatus.Failed(ex.Message);
            return false;
        }
    }
}
