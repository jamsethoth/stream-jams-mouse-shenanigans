namespace MouseShenanigans.Core;

public static class BuiltInRemappingProfiles
{
    public static RemappingProfile HorizontalInversion { get; } = new(
        "horizontal-inversion",
        left: new MovementVector(1, 0),
        right: new MovementVector(-1, 0),
        up: new MovementVector(0, -1),
        down: new MovementVector(0, 1));

    public static IReadOnlyList<RemappingProfile> All { get; } = [HorizontalInversion];
}
