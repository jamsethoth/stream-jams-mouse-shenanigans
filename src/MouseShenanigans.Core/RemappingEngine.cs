namespace MouseShenanigans.Core;

public static class RemappingEngine
{
    public static RemappedMouseDelta Remap(double dx, double dy, RemappingProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        DirectionalMovement movement = DirectionalMovement.FromDelta(dx, dy);

        return RemappedMouseDelta.Zero
            + (movement.Left * profile.Left)
            + (movement.Right * profile.Right)
            + (movement.Up * profile.Up)
            + (movement.Down * profile.Down);
    }
}
