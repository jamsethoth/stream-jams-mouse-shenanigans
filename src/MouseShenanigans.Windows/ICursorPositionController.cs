namespace MouseShenanigans.Windows;

public interface ICursorPositionController
{
    ScreenPoint GetPosition();

    void SetPosition(ScreenPoint targetPosition);
}
