namespace MouseShenanigans.Core;

public readonly record struct DirectionalMovement(double Left, double Right, double Up, double Down)
{
    public static DirectionalMovement FromDelta(double dx, double dy)
    {
        return new DirectionalMovement(
            Left: Math.Max(-dx, 0),
            Right: Math.Max(dx, 0),
            Up: Math.Max(-dy, 0),
            Down: Math.Max(dy, 0));
    }
}
