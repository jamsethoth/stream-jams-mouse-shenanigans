namespace MouseShenanigans.Core;

public sealed record RemappingProfile
{
    public RemappingProfile(
        string name,
        MovementVector left,
        MovementVector right,
        MovementVector up,
        MovementVector down)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Profile name must not be empty.", nameof(name));
        }

        Name = name;
        Left = left;
        Right = right;
        Up = up;
        Down = down;
    }

    public string Name { get; }

    public MovementVector Left { get; }

    public MovementVector Right { get; }

    public MovementVector Up { get; }

    public MovementVector Down { get; }
}
