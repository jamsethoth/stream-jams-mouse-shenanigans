namespace MouseShenanigans.Windows;

public readonly record struct ScreenPoint(int X, int Y)
{
    public ScreenPoint Offset(IntegerMouseDelta delta)
    {
        return new ScreenPoint(X + delta.Dx, Y + delta.Dy);
    }
}
