namespace MouseShenanigans.Windows;

public readonly record struct RuntimeMouseMovement
{
    public RuntimeMouseMovement(int dx, int dy, bool isInjected)
    {
        Dx = dx;
        Dy = dy;
        IsInjected = isInjected;
    }

    public int Dx { get; }

    public int Dy { get; }

    public bool IsInjected { get; }
}
