namespace MouseShenanigans.Windows;

public sealed class RuntimeTargetSelector
{
    private RuntimeTargetSelector(string? processName, string? windowTitleContains)
    {
        ProcessName = NormalizeProcessName(processName);
        WindowTitleContains = string.IsNullOrWhiteSpace(windowTitleContains)
            ? null
            : windowTitleContains.Trim();

        if (ProcessName is null && WindowTitleContains is null)
        {
            throw new ArgumentException("A runtime target must include a process name or window title match.");
        }
    }

    public string? ProcessName { get; }

    public string? WindowTitleContains { get; }

    public static RuntimeTargetSelector ForProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            throw new ArgumentException("Target process name must not be empty.", nameof(processName));
        }

        return new RuntimeTargetSelector(processName, windowTitleContains: null);
    }

    public static RuntimeTargetSelector ForWindowTitleContains(string windowTitleContains)
    {
        if (string.IsNullOrWhiteSpace(windowTitleContains))
        {
            throw new ArgumentException("Target window title text must not be empty.", nameof(windowTitleContains));
        }

        return new RuntimeTargetSelector(processName: null, windowTitleContains);
    }

    public bool IsMatch(TargetWindowSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return IsMatch(snapshot.ForegroundWindow) || IsMatch(snapshot.WindowUnderCursor);
    }

    private bool IsMatch(TargetWindowInfo? window)
    {
        if (window is null)
        {
            return false;
        }

        if (ProcessName is not null
            && string.Equals(NormalizeProcessName(window.ProcessName), ProcessName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return WindowTitleContains is not null
            && window.Title is not null
            && window.Title.Contains(WindowTitleContains, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeProcessName(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return null;
        }

        string trimmed = processName.Trim();
        return trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^4]
            : trimmed;
    }
}
