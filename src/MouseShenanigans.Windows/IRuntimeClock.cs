namespace MouseShenanigans.Windows;

public interface IRuntimeClock
{
    DateTimeOffset UtcNow { get; }
}
