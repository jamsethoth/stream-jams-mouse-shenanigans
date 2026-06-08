namespace MouseShenanigans.Windows;

public sealed record TargetWindowInfo
{
    public TargetWindowInfo(string? processName, string? title)
    {
        ProcessName = processName;
        Title = title;
    }

    public string? ProcessName { get; }

    public string? Title { get; }
}
