namespace MouseShenanigans.Tray;

public sealed class TraySingleInstanceGuard : IDisposable
{
    public const string DefaultMutexName = @"Local\MouseShenanigans.Tray";

    private readonly Mutex mutex;
    private bool disposed;

    private TraySingleInstanceGuard(Mutex mutex)
    {
        this.mutex = mutex;
    }

    public static bool TryAcquire(out TraySingleInstanceGuard? guard)
    {
        return TryAcquire(DefaultMutexName, out guard);
    }

    public static bool TryAcquire(string mutexName, out TraySingleInstanceGuard? guard)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mutexName);

        var mutex = new Mutex(false, mutexName);
        bool acquired;
        try
        {
            acquired = mutex.WaitOne(0);
        }
        catch (AbandonedMutexException)
        {
            acquired = true;
        }

        if (!acquired)
        {
            mutex.Dispose();
            guard = null;
            return false;
        }

        guard = new TraySingleInstanceGuard(mutex);
        return true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        mutex.ReleaseMutex();
        mutex.Dispose();
        disposed = true;
    }
}
