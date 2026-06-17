namespace MouseShenanigans.Windows.Tests;

public sealed class ManualRuntimeClock(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset utcNow = utcNow;

    public override DateTimeOffset GetUtcNow()
    {
        return utcNow;
    }

    public void Advance(TimeSpan duration)
    {
        utcNow += duration;
    }
}
