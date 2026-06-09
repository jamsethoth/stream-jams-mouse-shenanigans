namespace MouseShenanigans.Windows;

public sealed record TargetWindowInfo
{
    public TargetWindowInfo(string? processName, string? title, ScreenRectangle? bounds = null)
    {
        ProcessName = processName;
        Title = title;
        Bounds = bounds;
    }

    public string? ProcessName { get; }

    public string? Title { get; }

    public ScreenRectangle? Bounds { get; }
}
