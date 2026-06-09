namespace MouseShenanigans.Windows;

public sealed class AbsoluteCursorRemappingCoordinator : IRuntimeRemappingController
{
    private readonly RuntimeRemappingOptions options;
    private readonly IRawMouseMovementSource movementSource;
    private readonly ITargetWindowReader targetWindowReader;
    private readonly ICursorPositionController cursorPositionController;
    private readonly AbsoluteCursorRemappingDecisionEngine decisionEngine;
    private readonly bool isSupported;
    private bool disposed;

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
            WindowsRuntime.IsDesktopInputAvailable)
    {
    }

    public AbsoluteCursorRemappingCoordinator(
        RuntimeRemappingOptions options,
        IRawMouseMovementSource movementSource,
        ITargetWindowReader targetWindowReader,
        ICursorPositionController cursorPositionController,
        bool isSupported)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.movementSource = movementSource ?? throw new ArgumentNullException(nameof(movementSource));
        this.targetWindowReader = targetWindowReader ?? throw new ArgumentNullException(nameof(targetWindowReader));
        this.cursorPositionController = cursorPositionController ?? throw new ArgumentNullException(nameof(cursorPositionController));
        this.isSupported = isSupported;
        decisionEngine = new AbsoluteCursorRemappingDecisionEngine(
            options.ActiveProfile,
            options.AbsoluteCorrectionScale);

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
            movementSource.Start(HandleMovement);
            Status = RuntimeRemappingStatus.Enabled;
        }
        catch (Exception ex)
        {
            TryStopSource();
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

        if (TryStopSource())
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

        TryStopSource();
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
            bool targetMatches = options.TargetSelector.IsMatch(targetSnapshot);
            ScreenPoint currentPosition = cursorPositionController.GetPosition();
            AbsoluteCursorRemappingDecision decision = decisionEngine.Decide(
                new RuntimeMouseMovement(rawMovement.Dx, rawMovement.Dy, isInjected: false),
                isEnabled: true,
                targetMatches,
                currentPosition);

            if (decision.TargetPosition is { } targetPosition)
            {
                cursorPositionController.SetPosition(targetPosition);
            }
        }
        catch (Exception ex)
        {
            TryStopSource();
            Status = RuntimeRemappingStatus.Failed(ex.Message);
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
