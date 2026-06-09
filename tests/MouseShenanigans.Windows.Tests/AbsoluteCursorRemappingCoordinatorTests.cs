using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class AbsoluteCursorRemappingCoordinatorTests
{
    private static readonly ScreenRectangle TargetBounds = new(left: 0, top: 0, right: 200, bottom: 200);

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

    [Fact]
    public void HandleMovementPassesThroughTargetMatchOutsideBounds()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50))),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Empty(cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementResumesRemappingAfterReentryGracePeriod()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50)));
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        var clock = new ManualRuntimeClock(new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: reader,
            cursor: cursor,
            clock: clock);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = TargetSnapshot(new ScreenPoint(105, 50));
        cursor.Position = new ScreenPoint(105, 50);
        source.Raise(new IntegerMouseDelta(5, 0));
        clock.Advance(RuntimeRemappingOptions.DefaultTargetReentryGracePeriod);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(95, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementPassesThroughReenteredTargetDuringGracePeriod()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50)));
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        var clock = new ManualRuntimeClock(new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: reader,
            cursor: cursor,
            clock: clock);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = TargetSnapshot(new ScreenPoint(105, 50));
        cursor.Position = new ScreenPoint(105, 50);
        source.Raise(new IntegerMouseDelta(5, 0));
        clock.Advance(RuntimeRemappingOptions.DefaultTargetReentryGracePeriod - TimeSpan.FromMilliseconds(1));
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Empty(cursor.SetPositions);
    }

    [Fact]
    public void ConstructorStartsWithCursorLockDisabledByDefault()
    {
        using var coordinator = CreateCoordinator();

        Assert.False(coordinator.IsCursorLockEnabled);
    }

    [Fact]
    public void HandleMovementLocksCursorWhenLockEnabledAndTargetBoundsAreAvailable()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([TargetBounds], cursorLock.LockedBounds);
        Assert.Equal(0, cursorLock.ReleaseRequests);
    }

    [Fact]
    public void HandleMovementReleasesCursorLockWhenTargetIsLost()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(105, 50)));
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, targetWindowReader: reader, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = TargetWindowSnapshot.Empty;
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal(1, cursorLock.ReleaseRequests);
    }

    [Fact]
    public void DisableReleasesCursorLock()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();
        source.Raise(new IntegerMouseDelta(5, 0));

        coordinator.Disable();

        Assert.Equal(1, cursorLock.ReleaseRequests);
    }

    [Fact]
    public void DisposeReleasesCursorLock()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursorLock = new RecordingCursorLockController();
        var coordinator = CreateCoordinator(source: source, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();
        source.Raise(new IntegerMouseDelta(5, 0));

        coordinator.Dispose();

        Assert.Equal(1, cursorLock.ReleaseRequests);
    }


    [Fact]
    public void SetCursorLockEnabledFalseReleasesCursorLock()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();
        source.Raise(new IntegerMouseDelta(5, 0));

        coordinator.SetCursorLockEnabled(false);

        Assert.False(coordinator.IsCursorLockEnabled);
        Assert.Equal(1, cursorLock.ReleaseRequests);
    }

    [Fact]
    public void HandleMovementReleasesCursorLockWhenTargetBoundsBecomeUnavailable()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(105, 50)));
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, targetWindowReader: reader, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo("TargetApp", "Target App"),
            windowUnderCursor: null,
            cursorPosition: new ScreenPoint(105, 50));
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal(1, cursorLock.ReleaseRequests);
    }

    [Fact]
    public void HandleMovementReleasesCursorLockWhenRuntimeFails()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new ThrowingAfterFirstSnapshotTargetWindowReader(TargetSnapshot(new ScreenPoint(105, 50)));
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(source: source, targetWindowReader: reader, cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal(RuntimeRemappingState.Failed, coordinator.Status.State);
        Assert.Equal(1, cursorLock.ReleaseRequests);
    }

    private static AbsoluteCursorRemappingCoordinator CreateCoordinator(
        IRawMouseMovementSource? source = null,
        ITargetWindowReader? targetWindowReader = null,
        ICursorPositionController? cursor = null,
        ICursorLockController? cursorLock = null,
        IRuntimeClock? clock = null)
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            BuiltInRemappingProfiles.HorizontalInversion);

        return new AbsoluteCursorRemappingCoordinator(
            options,
            source ?? new RecordingRawMouseMovementSource(),
            targetWindowReader ?? new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(105, 50))),
            cursor ?? new RecordingCursorPositionController(new ScreenPoint(105, 50)),
            cursorLock ?? new RecordingCursorLockController(),
            clock ?? new ManualRuntimeClock(new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero)),
            isSupported: true);
    }

    private static TargetWindowSnapshot TargetSnapshot(ScreenPoint cursorPosition)
    {
        return new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo("TargetApp", "Target App", TargetBounds),
            windowUnderCursor: null,
            cursorPosition);
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

    private sealed class MutableTargetWindowReader(TargetWindowSnapshot snapshot) : ITargetWindowReader
    {
        public TargetWindowSnapshot Snapshot { get; set; } = snapshot;

        public TargetWindowSnapshot ReadSnapshot()
        {
            return Snapshot;
        }
    }

    private sealed class ThrowingAfterFirstSnapshotTargetWindowReader(TargetWindowSnapshot firstSnapshot) : ITargetWindowReader
    {
        private bool hasRead;

        public TargetWindowSnapshot ReadSnapshot()
        {
            if (hasRead)
            {
                throw new InvalidOperationException("Window reader failed.");
            }

            hasRead = true;
            return firstSnapshot;
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

    private sealed class RecordingCursorLockController : ICursorLockController
    {
        public List<ScreenRectangle> LockedBounds { get; } = [];

        public int ReleaseRequests { get; private set; }

        public void LockTo(ScreenRectangle bounds)
        {
            LockedBounds.Add(bounds);
        }

        public void Release()
        {
            ReleaseRequests++;
        }
    }
}
