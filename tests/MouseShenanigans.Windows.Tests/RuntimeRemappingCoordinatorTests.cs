using MouseShenanigans.Core;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class RuntimeRemappingCoordinatorTests
{
    [Fact]
    public void EnableStartsHookAndReportsEnabled()
    {
        var hook = new RecordingMouseMovementHook();
        using var coordinator = CreateCoordinator(hook: hook);

        coordinator.Enable();

        Assert.True(hook.IsStarted);
        Assert.Equal(RuntimeRemappingState.Enabled, coordinator.Status.State);
    }

    [Fact]
    public void DisableStopsHookAndReportsDisabled()
    {
        var hook = new RecordingMouseMovementHook();
        using var coordinator = CreateCoordinator(hook: hook);

        coordinator.Enable();
        coordinator.Disable();

        Assert.False(hook.IsStarted);
        Assert.Equal(RuntimeRemappingState.Disabled, coordinator.Status.State);
    }

    [Fact]
    public void EnableReportsFailedWhenHookCannotStart()
    {
        using var coordinator = CreateCoordinator(hook: new FailingMouseMovementHook());

        coordinator.Enable();

        Assert.Equal(RuntimeRemappingState.Failed, coordinator.Status.State);
    }

    [Fact]
    public void HandleMovementSuppressesTargetedPhysicalMovementAndInjectsReplacement()
    {
        var hook = new RecordingMouseMovementHook();
        var injector = new RecordingMouseMovementInjector();
        using var coordinator = CreateCoordinator(hook: hook, injector: injector);
        coordinator.Enable();

        bool suppressOriginal = hook.Raise(new RuntimeMouseMovement(dx: 4, dy: 0, isInjected: false));

        Assert.True(suppressOriginal);
        Assert.Equal([new IntegerMouseDelta(-4, 0)], injector.InjectedMovements);
    }

    [Fact]
    public void HandleMovementPassesThroughNonTargetMovementWithoutInjecting()
    {
        var hook = new RecordingMouseMovementHook();
        var injector = new RecordingMouseMovementInjector();
        using var coordinator = CreateCoordinator(
            hook: hook,
            targetWindowReader: new StubTargetWindowReader(TargetWindowSnapshot.Empty),
            injector: injector);
        coordinator.Enable();

        bool suppressOriginal = hook.Raise(new RuntimeMouseMovement(dx: 4, dy: 0, isInjected: false));

        Assert.False(suppressOriginal);
        Assert.Empty(injector.InjectedMovements);
    }

    [Fact]
    public void HandleMovementPassesThroughInjectedMovementWithoutInjectingAgain()
    {
        var hook = new RecordingMouseMovementHook();
        var injector = new RecordingMouseMovementInjector();
        using var coordinator = CreateCoordinator(hook: hook, injector: injector);
        coordinator.Enable();

        bool suppressOriginal = hook.Raise(new RuntimeMouseMovement(dx: -4, dy: 0, isInjected: true));

        Assert.False(suppressOriginal);
        Assert.Empty(injector.InjectedMovements);
    }

    private static RuntimeRemappingCoordinator CreateCoordinator(
        IMouseMovementHook? hook = null,
        ITargetWindowReader? targetWindowReader = null,
        IMouseMovementInjector? injector = null)
    {
        var options = new RuntimeRemappingOptions(
            RuntimeTargetSelector.ForProcessName("TargetApp"),
            BuiltInRemappingProfiles.HorizontalInversion);

        return new RuntimeRemappingCoordinator(
            options,
            hook ?? new RecordingMouseMovementHook(),
            targetWindowReader ?? new StubTargetWindowReader(new TargetWindowSnapshot(
                foregroundWindow: new TargetWindowInfo("TargetApp", "Target App"),
                windowUnderCursor: null)),
            injector ?? new RecordingMouseMovementInjector(),
            isSupported: true);
    }

    private sealed class RecordingMouseMovementHook : IMouseMovementHook
    {
        private Func<RuntimeMouseMovement, bool>? callback;

        public bool IsStarted { get; private set; }

        public void Start(Func<RuntimeMouseMovement, bool> onMovement)
        {
            callback = onMovement;
            IsStarted = true;
        }

        public void StopHook()
        {
            IsStarted = false;
        }

        public bool Raise(RuntimeMouseMovement movement)
        {
            return callback?.Invoke(movement) ?? false;
        }

        public void Dispose()
        {
            StopHook();
        }
    }

    private sealed class FailingMouseMovementHook : IMouseMovementHook
    {
        public void Start(Func<RuntimeMouseMovement, bool> onMovement)
        {
            throw new InvalidOperationException("No hook for you today.");
        }

        public void StopHook()
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class StubTargetWindowReader(TargetWindowSnapshot snapshot) : ITargetWindowReader
    {
        public TargetWindowSnapshot ReadSnapshot()
        {
            return snapshot;
        }
    }

    private sealed class RecordingMouseMovementInjector : IMouseMovementInjector
    {
        public List<IntegerMouseDelta> InjectedMovements { get; } = [];

        public void Inject(IntegerMouseDelta movement)
        {
            InjectedMovements.Add(movement);
        }
    }
}
