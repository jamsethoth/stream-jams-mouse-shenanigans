namespace MouseShenanigans.Windows;

public readonly record struct ScreenRectangle
{
    public ScreenRectangle(int left, int top, int right, int bottom)
    {
        if (right < left)
        {
            throw new ArgumentOutOfRangeException(nameof(right), "Right must be greater than or equal to left.");
        }

        if (bottom < top)
        {
            throw new ArgumentOutOfRangeException(nameof(bottom), "Bottom must be greater than or equal to top.");
        }

        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public int Left { get; }

    public int Top { get; }

    public int Right { get; }

    public int Bottom { get; }

    public bool Contains(ScreenPoint point)
    {
        return point.X >= Left
            && point.X < Right
            && point.Y >= Top
            && point.Y < Bottom;
    }

    public ScreenPoint Clamp(ScreenPoint point)
    {
        int maxX = Right > Left ? Right - 1 : Left;
        int maxY = Bottom > Top ? Bottom - 1 : Top;

        return new ScreenPoint(
            Math.Clamp(point.X, Left, maxX),
            Math.Clamp(point.Y, Top, maxY));
    }
}
