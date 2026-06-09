namespace MouseShenanigans.Windows;

public interface IRuntimeRemappingController : IDisposable
{
    RuntimeRemappingStatus Status { get; }

    void Enable();

    void Disable();
}
