namespace MouseShenanigans.Core;

public readonly record struct RemappedMouseDelta
{
    public RemappedMouseDelta(double dx, double dy)
    {
        if (!double.IsFinite(dx))
        {
            throw new ArgumentOutOfRangeException(nameof(dx), dx, "Remapped mouse delta x value must be finite.");
        }

        if (!double.IsFinite(dy))
        {
            throw new ArgumentOutOfRangeException(nameof(dy), dy, "Remapped mouse delta y value must be finite.");
        }

        Dx = dx;
        Dy = dy;
    }

    public double Dx { get; }

    public double Dy { get; }

    public static RemappedMouseDelta Zero { get; } = new(0, 0);

    public static RemappedMouseDelta operator +(RemappedMouseDelta left, RemappedMouseDelta right)
    {
        return new RemappedMouseDelta(left.Dx + right.Dx, left.Dy + right.Dy);
    }
}
