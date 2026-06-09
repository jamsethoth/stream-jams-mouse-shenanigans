namespace MouseShenanigans.Windows;

public sealed class SystemRuntimeClock : IRuntimeClock
{
    private SystemRuntimeClock()
    {
    }

    public static SystemRuntimeClock Instance { get; } = new();

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
