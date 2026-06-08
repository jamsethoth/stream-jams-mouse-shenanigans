namespace MouseShenanigans.Windows;

public interface IMouseMovementInjector
{
    void Inject(IntegerMouseDelta movement);
}
