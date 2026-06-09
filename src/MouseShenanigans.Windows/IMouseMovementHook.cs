namespace MouseShenanigans.Windows;

public interface IMouseMovementHook : IDisposable
{
    void Start(Func<RuntimeMouseMovement, bool> onMovement);

    void StopHook();
}
