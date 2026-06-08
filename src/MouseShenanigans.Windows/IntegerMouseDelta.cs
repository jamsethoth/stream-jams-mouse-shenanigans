namespace MouseShenanigans.Windows;

public readonly record struct IntegerMouseDelta(int Dx, int Dy)
{
    public static IntegerMouseDelta Zero { get; } = new(0, 0);

    public bool IsZero => Dx == 0 && Dy == 0;
}
