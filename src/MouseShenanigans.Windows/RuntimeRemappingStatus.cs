namespace MouseShenanigans.Windows;

public enum RuntimeRemappingState
{
    Unsupported,
    Disabled,
    Enabled,
    Failed,
}

public sealed record RuntimeRemappingStatus(RuntimeRemappingState State, string? Message = null)
{
    public static RuntimeRemappingStatus Unsupported(string message)
    {
        return new RuntimeRemappingStatus(RuntimeRemappingState.Unsupported, message);
    }

    public static RuntimeRemappingStatus Disabled { get; } = new(RuntimeRemappingState.Disabled);

    public static RuntimeRemappingStatus Enabled { get; } = new(RuntimeRemappingState.Enabled);

    public static RuntimeRemappingStatus Failed(string message)
    {
        return new RuntimeRemappingStatus(RuntimeRemappingState.Failed, message);
    }
}
