namespace MouseShenanigans.Windows;

public interface ICursorLockController
{
    void LockTo(ScreenRectangle bounds);

    void Release();
}
