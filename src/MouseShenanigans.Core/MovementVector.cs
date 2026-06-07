namespace MouseShenanigans.Core;

public readonly record struct MovementVector
{
    public MovementVector(double x, double y)
    {
        if (!double.IsFinite(x))
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Movement vector x value must be finite.");
        }

        if (!double.IsFinite(y))
        {
            throw new ArgumentOutOfRangeException(nameof(y), y, "Movement vector y value must be finite.");
        }

        X = x;
        Y = y;
    }

    public double X { get; }

    public double Y { get; }

    public static RemappedMouseDelta operator *(double magnitude, MovementVector vector)
    {
        return new RemappedMouseDelta(magnitude * vector.X, magnitude * vector.Y);
    }
}
