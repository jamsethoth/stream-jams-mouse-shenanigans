namespace MouseShenanigans.Windows;

public interface IRawMouseMovementSource : IDisposable
{
    void Start(Action<IntegerMouseDelta> onMovement);

    void StopObservation();
}
