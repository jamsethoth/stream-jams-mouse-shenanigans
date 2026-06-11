namespace MouseShenanigans.Tray.Tests;

public sealed class TraySingleInstanceGuardTests
{
    [Fact]
    public void TryAcquireRejectsSecondOwnerUntilFirstOwnerDisposes()
    {
        string mutexName = $@"Local\MouseShenanigans.Tray.Tests.{Guid.NewGuid():N}";

        Assert.True(TraySingleInstanceGuard.TryAcquire(mutexName, out TraySingleInstanceGuard? first));
        Assert.NotNull(first);
        using (first)
        {
            bool secondAcquired = true;
            TraySingleInstanceGuard? second = null;
            var thread = new Thread(() => secondAcquired = TraySingleInstanceGuard.TryAcquire(mutexName, out second));
            thread.Start();
            thread.Join();

            second?.Dispose();

            Assert.False(secondAcquired);
            Assert.Null(second);
        }

        Assert.True(TraySingleInstanceGuard.TryAcquire(mutexName, out TraySingleInstanceGuard? reacquired));
        Assert.NotNull(reacquired);
        reacquired.Dispose();
    }
}
