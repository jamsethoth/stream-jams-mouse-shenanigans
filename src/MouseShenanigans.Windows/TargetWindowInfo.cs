namespace MouseShenanigans.Windows;

public sealed record TargetWindowInfo
{
    public TargetWindowInfo(string? processName, string? title, ScreenRectangle? bounds = null, string? executablePath = null)
    {
        ProcessName = processName;
        Title = title;
        Bounds = bounds;
        ExecutablePath = executablePath;
    }

    public string? ProcessName { get; }

    public string? ExecutablePath { get; }

    public string? Title { get; }

    public ScreenRectangle? Bounds { get; }
}
