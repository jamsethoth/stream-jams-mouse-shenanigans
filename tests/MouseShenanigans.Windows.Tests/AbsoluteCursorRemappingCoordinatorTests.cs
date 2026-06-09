using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class AbsoluteCursorRemappingCoordinatorTests
{
    [Fact]
    public void EnableStartsRawInputSourceAndReportsEnabled()
    {
        var source = new RecordingRawMouseMovementSource();
        using var coordinator = CreateCoordinator(source: source);

        coordinator.Enable();

        Assert.True(source.IsStarted);
        Assert.Equal(RuntimeRemappingState.Enabled, coordinator.Status.State);
    }

    [Fact]
    public void DisableStopsRawInputSourceAndReportsDisabled()
    {
        var source = new RecordingRawMouseMovementSource();
        using var coordinator = CreateCoordinator(source: source);

        coordinator.Enable();
        coordinator.Disable();

        Assert.False(source.IsStarted);
        Assert.Equal(RuntimeRemappingState.Disabled, coordinator.Status.State);
    }

    [Fact]
    public void HandleMovementSetsAbsoluteCursorPositionForTargetedMovement()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(100, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(105, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(95, 50)], cursor.SetPositions);
    }


    [Fact]
    public void HandleMovementKeepsCorrectionBoundedToRawInputMagnitudeWhenScreenMovementIsAccelerated()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(100, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(120, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(110, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementPassesThroughNonTargetMovement()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(105, 50));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetWindowSnapshot.Empty),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Empty(cursor.SetPositions);
    }

    private static AbsoluteCursorRemappingCoordinator CreateCoordinator(
        IRawMouseMovementSource? source = null,
        ITargetWindowReader? targetWindowReader = null,
        ICursorPositionController? cursor = null)
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            BuiltInRemappingProfiles.HorizontalInversion);

        return new AbsoluteCursorRemappingCoordinator(
            options,
            source ?? new RecordingRawMouseMovementSource(),
            targetWindowReader ?? new StubTargetWindowReader(new TargetWindowSnapshot(
                foregroundWindow: new TargetWindowInfo("TargetApp", "Target App"),
                windowUnderCursor: null)),
            cursor ?? new RecordingCursorPositionController(new ScreenPoint(105, 50)),
            isSupported: true);
    }

    private sealed class RecordingRawMouseMovementSource : IRawMouseMovementSource
    {
        private Action<IntegerMouseDelta>? callback;

        public bool IsStarted { get; private set; }

        public void Start(Action<IntegerMouseDelta> onMovement)
        {
            callback = onMovement;
            IsStarted = true;
        }

        public void StopObservation()
        {
            IsStarted = false;
        }

        public void Raise(IntegerMouseDelta movement)
        {
            callback?.Invoke(movement);
        }

        public void Dispose()
        {
            StopObservation();
        }
    }

    private sealed class StubTargetWindowReader(TargetWindowSnapshot snapshot) : ITargetWindowReader
    {
        public TargetWindowSnapshot ReadSnapshot()
        {
            return snapshot;
        }
    }

    private sealed class RecordingCursorPositionController(ScreenPoint position) : ICursorPositionController
    {
        public ScreenPoint Position { get; set; } = position;

        public List<ScreenPoint> SetPositions { get; } = [];

        public ScreenPoint GetPosition()
        {
            return Position;
        }

        public void SetPosition(ScreenPoint targetPosition)
        {
            SetPositions.Add(targetPosition);
            Position = targetPosition;
        }
    }
}
