using MouseShenanigans.Windows;

namespace MouseShenanigans.Windows.Tests;

public sealed class ManualRuntimeClock(DateTimeOffset utcNow) : IRuntimeClock
{
    public DateTimeOffset UtcNow { get; private set; } = utcNow;

    public void Advance(TimeSpan duration)
    {
        UtcNow += duration;
    }
}
