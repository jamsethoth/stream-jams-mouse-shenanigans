using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class AbsoluteCursorRemappingCoordinatorTests
{
    private static readonly ScreenRectangle TargetBounds = new(left: 0, top: 0, right: 200, bottom: 200);
    private static readonly ScreenRectangle VirtualScreenBounds = ReadVirtualScreenBounds();

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
    public void HandleMovementInvertsObservedScreenMovementWhenScreenMovementIsAccelerated()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(100, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(120, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(80, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementCapsObservedScreenMovementWhenItExceedsRawInputLimit()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(100, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(120, 50);
        source.Raise(new IntegerMouseDelta(1, 0));

        Assert.Equal([new ScreenPoint(92, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementClampsTargetPositionToTargetBounds()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(190, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(150, 50);
        source.Raise(new IntegerMouseDelta(-10, 0));

        Assert.Equal([new ScreenPoint(199, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtRightBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(199, 50));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(199, 50))),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(194, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtLeftBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(0, 50));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(0, 50))),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(-5, 0));

        Assert.Equal([new ScreenPoint(5, 50)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtVirtualLeftBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        int y = VirtualScreenBounds.Top + 100;
        var cursor = new RecordingCursorPositionController(new ScreenPoint(VirtualScreenBounds.Left, y));
        ScreenRectangle targetBounds = new(
            VirtualScreenBounds.Left - 8,
            VirtualScreenBounds.Top,
            VirtualScreenBounds.Left + 200,
            VirtualScreenBounds.Top + 200);
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(VirtualScreenBounds.Left, y), targetBounds)),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(-5, 0));

        Assert.Equal([new ScreenPoint(VirtualScreenBounds.Left + 5, y)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtVirtualRightBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        int rightEdgeX = VirtualScreenBounds.Right - 1;
        int y = VirtualScreenBounds.Top + 100;
        var cursor = new RecordingCursorPositionController(new ScreenPoint(rightEdgeX, y));
        ScreenRectangle targetBounds = new(
            VirtualScreenBounds.Right - 200,
            VirtualScreenBounds.Top,
            VirtualScreenBounds.Right + 8,
            VirtualScreenBounds.Top + 200);
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(rightEdgeX, y), targetBounds)),
            cursor: cursor);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(rightEdgeX - 5, y)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtVirtualTopBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        int x = VirtualScreenBounds.Left + 100;
        var cursor = new RecordingCursorPositionController(new ScreenPoint(x, VirtualScreenBounds.Top));
        ScreenRectangle targetBounds = new(
            VirtualScreenBounds.Left,
            VirtualScreenBounds.Top - 8,
            VirtualScreenBounds.Left + 200,
            VirtualScreenBounds.Top + 200);
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(x, VirtualScreenBounds.Top), targetBounds)),
            cursor: cursor,
            profile: CreateVerticalInversionProfile());
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(0, -5));

        Assert.Equal([new ScreenPoint(x, VirtualScreenBounds.Top + 5)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementUsesRawDeltaWhenCursorIsPinnedAtVirtualBottomBoundary()
    {
        var source = new RecordingRawMouseMovementSource();
        int bottomEdgeY = VirtualScreenBounds.Bottom - 1;
        int x = VirtualScreenBounds.Left + 100;
        var cursor = new RecordingCursorPositionController(new ScreenPoint(x, bottomEdgeY));
        ScreenRectangle targetBounds = new(
            VirtualScreenBounds.Left,
            VirtualScreenBounds.Bottom - 200,
            VirtualScreenBounds.Left + 200,
            VirtualScreenBounds.Bottom + 8);
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(x, bottomEdgeY), targetBounds)),
            cursor: cursor,
            profile: CreateVerticalInversionProfile());
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(0, 5));

        Assert.Equal([new ScreenPoint(x, bottomEdgeY - 5)], cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementPassesThroughWhenObservedMovementIsTooLargeForRawInput()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(20, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(180, 50);
        source.Raise(new IntegerMouseDelta(1, 0));

        Assert.Empty(cursor.SetPositions);
    }

    [Fact]
    public void HandleMovementPassesThroughStaleCursorAfterClampedTarget()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(190, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        cursor.Position = new ScreenPoint(150, 50);
        source.Raise(new IntegerMouseDelta(-10, 0));
        cursor.Position = new ScreenPoint(20, 50);
        source.Raise(new IntegerMouseDelta(-1, 0));

        Assert.Equal([new ScreenPoint(199, 50)], cursor.SetPositions);
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
    public void HandleMovementClampsBackInsideTargetWhenCursorLockEnabledAndCursorEscapesBounds()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50))),
            cursor: cursor,
            cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(199, 50)], cursor.SetPositions);
        Assert.Equal([TargetBounds], cursorLock.LockedBounds);
        Assert.Equal(0, cursorLock.ReleaseRequests);
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
        cursor.Position = new ScreenPoint(110, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(100, 50)], cursor.SetPositions);
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
    public void HandleMovementRemapsImmediatelyAfterReentryWhenCursorLockEnabled()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50)));
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: reader,
            cursor: cursor);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = TargetSnapshot(new ScreenPoint(170, 50));
        cursor.Position = new ScreenPoint(170, 50);
        source.Raise(new IntegerMouseDelta(-5, 0));

        Assert.Equal([new ScreenPoint(199, 50), new ScreenPoint(199, 50)], cursor.SetPositions);
    }

    [Fact]
    public void ConstructorUsesOptionsCursorLockSetting()
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
    public void HandleMovementClipsCursorLockToVirtualScreenBounds()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursorLock = new RecordingCursorLockController();
        int x = VirtualScreenBounds.Left + 100;
        ScreenRectangle targetBounds = new(
            VirtualScreenBounds.Left,
            VirtualScreenBounds.Top - 8,
            VirtualScreenBounds.Left + 200,
            VirtualScreenBounds.Top + 200);
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: new StubTargetWindowReader(TargetSnapshot(new ScreenPoint(x, VirtualScreenBounds.Top), targetBounds)),
            cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenRectangle(
            targetBounds.Left,
            VirtualScreenBounds.Top,
            targetBounds.Right,
            targetBounds.Bottom)], cursorLock.LockedBounds);
    }

    [Fact]
    public void HandleMovementRetainsCursorLockWhenTargetMatchIsTransientlyLost()
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

        Assert.Equal(0, cursorLock.ReleaseRequests);
        Assert.Equal([TargetBounds], cursorLock.LockedBounds);
    }

    [Fact]
    public void HandleMovementClampsBackInsideActiveLockWhenTargetMatchIsTransientlyLost()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(105, 50)));
        var cursor = new RecordingCursorPositionController(new ScreenPoint(105, 50));
        var cursorLock = new RecordingCursorLockController();
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: reader,
            cursor: cursor,
            cursorLock: cursorLock);
        coordinator.SetCursorLockEnabled(true);
        coordinator.Enable();

        source.Raise(new IntegerMouseDelta(5, 0));
        reader.Snapshot = TargetWindowSnapshot.Empty;
        cursor.Position = new ScreenPoint(250, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Contains(new ScreenPoint(199, 50), cursor.SetPositions);
        Assert.Equal(0, cursorLock.ReleaseRequests);
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

    [Fact]
    public void ApplyOptionsUsesNewProfileForLaterEligibleMovement()
    {
        var source = new RecordingRawMouseMovementSource();
        var cursor = new RecordingCursorPositionController(new ScreenPoint(105, 50));
        using var coordinator = CreateCoordinator(source: source, cursor: cursor);
        coordinator.Enable();

        coordinator.ApplyOptions(new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            new RemappingProfile(
                "double-right",
                left: new MovementVector(-1, 0),
                right: new MovementVector(2, 0),
                up: new MovementVector(0, -1),
                down: new MovementVector(0, 1))));

        cursor.Position = new ScreenPoint(110, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(115, 50)], cursor.SetPositions);
    }

    [Fact]
    public void ApplyOptionsWhileEnabledPreservesReentryGraceFromCurrentOutsideTargetState()
    {
        var source = new RecordingRawMouseMovementSource();
        var reader = new MutableTargetWindowReader(TargetSnapshot(new ScreenPoint(250, 50)));
        var cursor = new RecordingCursorPositionController(new ScreenPoint(250, 50));
        var clock = new ManualRuntimeClock(new DateTimeOffset(2026, 6, 12, 12, 0, 0, TimeSpan.Zero));
        using var coordinator = CreateCoordinator(
            source: source,
            targetWindowReader: reader,
            cursor: cursor,
            clock: clock);
        coordinator.Enable();

        coordinator.ApplyOptions(new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            RuntimeProofOfConceptDefaults.HorizontalInversionProfile));
        reader.Snapshot = TargetSnapshot(new ScreenPoint(105, 50));
        cursor.Position = new ScreenPoint(105, 50);

        source.Raise(new IntegerMouseDelta(5, 0));
        clock.Advance(RuntimeRemappingOptions.DefaultTargetReentryGracePeriod);
        cursor.Position = new ScreenPoint(110, 50);
        source.Raise(new IntegerMouseDelta(5, 0));

        Assert.Equal([new ScreenPoint(100, 50)], cursor.SetPositions);
    }

    private static AbsoluteCursorRemappingCoordinator CreateCoordinator(
        IRawMouseMovementSource? source = null,
        ITargetWindowReader? targetWindowReader = null,
        ICursorPositionController? cursor = null,
        ICursorLockController? cursorLock = null,
        TimeProvider? clock = null,
        RemappingProfile? profile = null)
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            profile ?? RuntimeProofOfConceptDefaults.HorizontalInversionProfile);

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
        return TargetSnapshot(cursorPosition, TargetBounds);
    }

    private static TargetWindowSnapshot TargetSnapshot(ScreenPoint cursorPosition, ScreenRectangle targetBounds)
    {
        return new TargetWindowSnapshot(
            foregroundWindow: new TargetWindowInfo("TargetApp", "Target App", targetBounds),
            windowUnderCursor: null,
            cursorPosition);
    }

    private static RemappingProfile CreateVerticalInversionProfile()
    {
        return new RemappingProfile(
            "vertical-inversion",
            left: new MovementVector(-1, 0),
            right: new MovementVector(1, 0),
            up: new MovementVector(0, 1),
            down: new MovementVector(0, -1));
    }

    private static ScreenRectangle ReadVirtualScreenBounds()
    {
        System.Drawing.Rectangle bounds = System.Windows.Forms.SystemInformation.VirtualScreen;
        return new ScreenRectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
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
