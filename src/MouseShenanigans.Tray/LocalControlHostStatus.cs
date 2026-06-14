namespace MouseShenanigans.Tray;

public enum LocalControlHostState
{
    Stopped,
    Available,
    Failed,
}

public sealed record LocalControlHostStatus(LocalControlHostState State, string? Url = null, string? Message = null)
{
    public static LocalControlHostStatus Stopped { get; } = new(LocalControlHostState.Stopped);

    public static LocalControlHostStatus Available(string url)
    {
        return new LocalControlHostStatus(LocalControlHostState.Available, url, $"Local control available at {url}");
    }

    public static LocalControlHostStatus Failed(string message)
    {
        return new LocalControlHostStatus(LocalControlHostState.Failed, Message: $"Local control unavailable: {message}");
    }
}
