namespace MouseShenanigans.Windows;

public sealed class RuntimeTargetSelector
{
    private RuntimeTargetSelector(string? processName, string? executablePath, string? windowTitleContains)
    {
        ProcessName = NormalizeProcessName(processName);
        ExecutablePath = NormalizeExecutablePath(executablePath);
        WindowTitleContains = string.IsNullOrWhiteSpace(windowTitleContains)
            ? null
            : windowTitleContains.Trim();

        if (ProcessName is null && ExecutablePath is null && WindowTitleContains is null)
        {
            throw new ArgumentException("A runtime target must include a process name, executable path, or window title match.");
        }
    }

    public string? ProcessName { get; }

    public string? ExecutablePath { get; }

    public string? WindowTitleContains { get; }

    public static RuntimeTargetSelector ForProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            throw new ArgumentException("Target process name must not be empty.", nameof(processName));
        }

        return new RuntimeTargetSelector(processName, executablePath: null, windowTitleContains: null);
    }

    public static RuntimeTargetSelector ForApplicationIdentity(ApplicationIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return new RuntimeTargetSelector(identity.ProcessName, identity.ExecutablePath, identity.WindowTitleContains);
    }

    public static RuntimeTargetSelector ForWindowTitleContains(string windowTitleContains)
    {
        if (string.IsNullOrWhiteSpace(windowTitleContains))
        {
            throw new ArgumentException("Target window title text must not be empty.", nameof(windowTitleContains));
        }

        return new RuntimeTargetSelector(processName: null, executablePath: null, windowTitleContains);
    }

    public static RuntimeTargetSelector Create(string? processName, string? windowTitleContains)
    {
        return Create(processName, executablePath: null, windowTitleContains);
    }

    public static RuntimeTargetSelector Create(string? processName, string? executablePath, string? windowTitleContains)
    {
        return new RuntimeTargetSelector(processName, executablePath, windowTitleContains);
    }

    public RuntimeTargetEligibility Evaluate(TargetWindowSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        RuntimeTargetEligibility? fallback = null;

        foreach (TargetWindowInfo window in MatchingWindows(snapshot))
        {
            RuntimeTargetEligibility eligibility = EvaluateMatchedWindow(window, snapshot.CursorPosition);
            if (eligibility.State == RuntimeTargetEligibilityState.InsideBounds)
            {
                return eligibility;
            }

            fallback ??= eligibility;
        }

        return fallback ?? RuntimeTargetEligibility.NoMatch;
    }

    public bool IsMatch(TargetWindowSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return IsIdentityMatch(snapshot.ForegroundWindow) || IsIdentityMatch(snapshot.WindowUnderCursor);
    }

    private IEnumerable<TargetWindowInfo> MatchingWindows(TargetWindowSnapshot snapshot)
    {
        if (snapshot.ForegroundWindow is { } foregroundWindow
            && IsIdentityMatch(foregroundWindow))
        {
            yield return foregroundWindow;
        }

        if (snapshot.WindowUnderCursor is { } windowUnderCursor
            && !ReferenceEquals(windowUnderCursor, snapshot.ForegroundWindow)
            && IsIdentityMatch(windowUnderCursor))
        {
            yield return windowUnderCursor;
        }
    }

    private static RuntimeTargetEligibility EvaluateMatchedWindow(
        TargetWindowInfo window,
        ScreenPoint? cursorPosition)
    {
        if (window.Bounds is not { } bounds || cursorPosition is not { } point)
        {
            return RuntimeTargetEligibility.BoundsUnavailable(window);
        }

        return bounds.Contains(point)
            ? RuntimeTargetEligibility.InsideBounds(window)
            : RuntimeTargetEligibility.OutsideBounds(window);
    }

    private bool IsIdentityMatch(TargetWindowInfo? window)
    {
        if (window is null)
        {
            return false;
        }

        if (ProcessName is not null
            && string.Equals(NormalizeProcessName(window.ProcessName), ProcessName, StringComparison.OrdinalIgnoreCase))
        {
            if (ExecutablePath is null || string.Equals(window.ExecutablePath, ExecutablePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (ProcessName is null
            && ExecutablePath is not null
            && string.Equals(window.ExecutablePath, ExecutablePath, StringComparison.OrdinalIgnoreCase))
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

    private static string? NormalizeExecutablePath(string? executablePath)
    {
        return ApplicationIdentity.NormalizeExecutablePath(executablePath);
    }
}
