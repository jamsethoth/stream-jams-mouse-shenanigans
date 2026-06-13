namespace MouseShenanigans.Windows;

public readonly record struct RuntimeMouseMovement
{
    public RuntimeMouseMovement(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }

    public int Dx { get; }

    public int Dy { get; }
}
