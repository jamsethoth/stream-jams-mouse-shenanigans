namespace MouseShenanigans.Windows;

public sealed class AbsoluteCursorRemappingCoordinator : IRuntimeRemappingController
{
    private const int BoundaryFallbackTolerancePixels = 1;

    private readonly IRawMouseMovementSource movementSource;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly ICursorPositionController cursorPositionController;
    private readonly ICursorLockController cursorLockController;
    private readonly RuntimeTargetReentryGate targetReentryGate;
    private readonly bool isSupported;
    private RuntimeRemappingOptions options;
    private AbsoluteCursorRemappingDecisionEngine decisionEngine;
    private bool disposed;
    private bool isCursorLockEnabled;
    private ScreenRectangle? activeCursorLockBounds;
    private ScreenPoint? lastAcceptedCursorPosition;
    private ScreenRectangle? lastAcceptedTargetBounds;

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController,
        ICursorLockController cursorLockController,
        TimeProvider clock,
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

        if (isCursorLockEnabled != enabled)
        {
            ResetAcceptedCursorPosition();
            targetReentryGate.Reset();
        }

        isCursorLockEnabled = enabled;
        if (!enabled)
        {
            TryReleaseCursorLock();
        }
    }

    public void ApplyOptions(RuntimeRemappingOptions options)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;
        isCursorLockEnabled = options.CursorLockEnabled;
        decisionEngine = new AbsoluteCursorRemappingDecisionEngine(
            options.ActiveProfile,
            options.AbsoluteCorrectionScale);
        decisionEngine.ResetAccumulator();
        ResetAcceptedCursorPosition();
        ResetTargetStateAfterOptionsChanged();
    }

    private void ResetTargetStateAfterOptionsChanged()
    {
        bool wasEnabled = Status.State == RuntimeRemappingState.Enabled;
        targetReentryGate.Reset();
        TryReleaseCursorLock();

        if (!wasEnabled)
        {
            return;
        }

        try
        {
            TargetWindowSnapshot targetSnapshot = targetWindowReader.ReadSnapshot();
            RuntimeTargetEligibility eligibility = options.TargetSelector.Evaluate(targetSnapshot);
            RuntimeTargetReadiness readiness = EvaluateReadiness(eligibility);
            UpdateCursorLock(readiness.Eligibility);
            SeedAcceptedCursorPosition(readiness.Eligibility.TargetBounds);
        }
        catch (Exception ex)
        {
            TryStopSource();
            TryReleaseCursorLock();
            targetReentryGate.Reset();
            ResetAcceptedCursorPosition();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
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
            ResetAcceptedCursorPosition();
            SeedAcceptedCursorPosition(targetBounds: null);
            movementSource.Start(HandleMovement);
            Status = RuntimeRemappingStatus.Enabled;
        }
        catch (Exception ex)
        {
            TryStopSource();
            TryReleaseCursorLock();
            targetReentryGate.Reset();
            ResetAcceptedCursorPosition();
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
            ResetAcceptedCursorPosition();
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
        ResetAcceptedCursorPosition();
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
            RuntimeTargetReadiness readiness = EvaluateReadiness(eligibility);

            ScreenPoint currentPosition = cursorPositionController.GetPosition();
            bool retainedCursorLockAfterNoMatch = ShouldRetainCursorLockAfterNoMatch(readiness.Eligibility);
            if (!retainedCursorLockAfterNoMatch)
            {
                UpdateCursorLock(readiness.Eligibility);
            }

            ScreenRectangle? targetBounds = readiness.Eligibility.TargetBounds
                ?? (retainedCursorLockAfterNoMatch ? activeCursorLockBounds : null);
            bool isOutsideTargetBounds = targetBounds is { } bounds
                && !bounds.Contains(currentPosition);

            if (isCursorLockEnabled
                && isOutsideTargetBounds
                && targetBounds is { } escapeBounds)
            {
                ScreenPoint targetPosition = escapeBounds.Clamp(currentPosition);
                if (targetPosition != currentPosition)
                {
                    cursorPositionController.SetPosition(targetPosition);
                }

                AcceptCursorPosition(targetPosition, targetBounds);
            }
            else if (!readiness.IsEligibleForRemapping)
            {
                AcceptCursorPosition(currentPosition, targetBounds);
            }
            else
            {
                ScreenPoint anchor = GetAnchorPosition(currentPosition, targetBounds);
                var observedMovement = new RuntimeMouseMovement(
                    currentPosition.X - anchor.X,
                    currentPosition.Y - anchor.Y);
                RuntimeMouseMovement effectiveMovement = CreateBoundaryAwareMovement(
                    rawMovement,
                    observedMovement,
                    currentPosition,
                    targetBounds);

                if (TryCreateBoundedMovement(rawMovement, effectiveMovement, out RuntimeMouseMovement boundedMovement))
                {
                    AbsoluteCursorRemappingDecision decision = decisionEngine.Decide(
                        boundedMovement,
                        isEnabled: true,
                        targetMatches: true,
                        anchor);
                    ScreenPoint? targetPosition = ClampToTargetBounds(decision.TargetPosition, targetBounds);

                    if (targetPosition is { } target)
                    {
                        cursorPositionController.SetPosition(target);
                        AcceptCursorPosition(target, targetBounds);
                    }
                    else
                    {
                        AcceptCursorPosition(currentPosition, targetBounds);
                    }
                }
                else
                {
                    AcceptCursorPosition(currentPosition, targetBounds);
                }
            }
        }
        catch (Exception ex)
        {
            TryStopSource();
            TryReleaseCursorLock();
            targetReentryGate.Reset();
            ResetAcceptedCursorPosition();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
        }
    }

    private void ResetAcceptedCursorPosition()
    {
        lastAcceptedCursorPosition = null;
        lastAcceptedTargetBounds = null;
    }

    private void SeedAcceptedCursorPosition(ScreenRectangle? targetBounds)
    {
        AcceptCursorPosition(cursorPositionController.GetPosition(), targetBounds);
    }

    private ScreenPoint GetAnchorPosition(ScreenPoint currentPosition, ScreenRectangle? targetBounds)
    {
        if (lastAcceptedCursorPosition is not { } anchorPosition)
        {
            AcceptCursorPosition(currentPosition, targetBounds);
            return currentPosition;
        }

        if (lastAcceptedTargetBounds is { } previousTargetBounds
            && previousTargetBounds != targetBounds)
        {
            AcceptCursorPosition(currentPosition, targetBounds);
            return currentPosition;
        }

        lastAcceptedTargetBounds = targetBounds;
        return anchorPosition;
    }

    private RuntimeTargetReadiness EvaluateReadiness(RuntimeTargetEligibility eligibility)
    {
        if (isCursorLockEnabled)
        {
            return new RuntimeTargetReadiness(eligibility, eligibility.IsEligibleForRemapping);
        }

        return targetReentryGate.Evaluate(eligibility);
    }

    private bool ShouldRetainCursorLockAfterNoMatch(RuntimeTargetEligibility eligibility)
    {
        return isCursorLockEnabled
            && activeCursorLockBounds is not null
            && eligibility.State == RuntimeTargetEligibilityState.NoMatch;
    }

    private void AcceptCursorPosition(ScreenPoint position, ScreenRectangle? targetBounds)
    {
        lastAcceptedCursorPosition = position;
        lastAcceptedTargetBounds = targetBounds;
    }

    private static ScreenPoint? ClampToTargetBounds(ScreenPoint? targetPosition, ScreenRectangle? targetBounds)
    {
        if (targetPosition is not { } position)
        {
            return null;
        }

        return targetBounds?.Clamp(position) ?? position;
    }

    private static bool TryCreateBoundedMovement(
        IntegerMouseDelta rawMovement,
        RuntimeMouseMovement movement,
        out RuntimeMouseMovement boundedMovement)
    {
        boundedMovement = new RuntimeMouseMovement(0, 0);

        if (IsObservedMovementStale(rawMovement, movement))
        {
            return false;
        }

        boundedMovement = new RuntimeMouseMovement(
            ClampDelta(movement.Dx, CreateObservedMovementLimit(rawMovement.Dx)),
            ClampDelta(movement.Dy, CreateObservedMovementLimit(rawMovement.Dy)));
        return true;
    }

    private static RuntimeMouseMovement CreateBoundaryAwareMovement(
        IntegerMouseDelta rawMovement,
        RuntimeMouseMovement observedMovement,
        ScreenPoint currentPosition,
        ScreenRectangle? targetBounds)
    {
        if (targetBounds is not { } bounds)
        {
            return observedMovement;
        }

        int dx = observedMovement.Dx;
        int dy = observedMovement.Dy;

        if ((IsNearLeftBoundary(currentPosition, bounds) && rawMovement.Dx < 0)
            || (IsNearRightBoundary(currentPosition, bounds) && rawMovement.Dx > 0))
        {
            dx = rawMovement.Dx;
        }

        if ((IsNearTopBoundary(currentPosition, bounds) && rawMovement.Dy < 0)
            || (IsNearBottomBoundary(currentPosition, bounds) && rawMovement.Dy > 0))
        {
            dy = rawMovement.Dy;
        }

        return new RuntimeMouseMovement(dx, dy);
    }

    private static bool IsNearLeftBoundary(ScreenPoint position, ScreenRectangle bounds)
    {
        return position.X <= bounds.Left + BoundaryFallbackTolerancePixels;
    }

    private static bool IsNearRightBoundary(ScreenPoint position, ScreenRectangle bounds)
    {
        return position.X >= GetMaxX(bounds) - BoundaryFallbackTolerancePixels;
    }

    private static bool IsNearTopBoundary(ScreenPoint position, ScreenRectangle bounds)
    {
        return position.Y <= bounds.Top + BoundaryFallbackTolerancePixels;
    }

    private static bool IsNearBottomBoundary(ScreenPoint position, ScreenRectangle bounds)
    {
        return position.Y >= GetMaxY(bounds) - BoundaryFallbackTolerancePixels;
    }

    private static int GetMaxX(ScreenRectangle bounds)
    {
        return bounds.Right > bounds.Left ? bounds.Right - 1 : bounds.Left;
    }

    private static int GetMaxY(ScreenRectangle bounds)
    {
        return bounds.Bottom > bounds.Top ? bounds.Bottom - 1 : bounds.Top;
    }

    private static int CreateObservedMovementLimit(int rawDelta)
    {
        return Math.Max(Math.Abs(rawDelta) * 4, 8);
    }

    private static bool IsObservedMovementStale(IntegerMouseDelta rawMovement, RuntimeMouseMovement observedMovement)
    {
        return Math.Abs(observedMovement.Dx) > CreateStaleObservedMovementLimit(rawMovement.Dx)
            || Math.Abs(observedMovement.Dy) > CreateStaleObservedMovementLimit(rawMovement.Dy);
    }

    private static int CreateStaleObservedMovementLimit(int rawDelta)
    {
        return Math.Max(Math.Abs(rawDelta) * 16, 64);
    }

    private static int ClampDelta(int delta, int absoluteLimit)
    {
        return Math.Clamp(delta, -absoluteLimit, absoluteLimit);
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
