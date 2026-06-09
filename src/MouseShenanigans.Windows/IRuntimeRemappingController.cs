namespace MouseShenanigans.Windows;

public interface IRuntimeRemappingController : IDisposable
{
    RuntimeRemappingStatus Status { get; }

    bool IsCursorLockEnabled { get; }

    void SetCursorLockEnabled(bool enabled);

    void Enable();

    void Disable();
}
