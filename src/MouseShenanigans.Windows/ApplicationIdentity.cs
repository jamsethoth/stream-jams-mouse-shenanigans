namespace MouseShenanigans.Windows;

public sealed record ApplicationIdentity
{
    public ApplicationIdentity(string? processName, string? executablePath = null, string? windowTitleContains = null)
    {
        ProcessName = NormalizeProcessName(processName);
        ExecutablePath = NormalizeExecutablePath(executablePath);
        WindowTitleContains = string.IsNullOrWhiteSpace(windowTitleContains)
            ? null
            : windowTitleContains.Trim();

        if (ProcessName is null && ExecutablePath is null && WindowTitleContains is null)
        {
            throw new ArgumentException(
                "Application identity must include a process name, executable path, or window title match.");
        }
    }

    public string? ProcessName { get; }

    public string? ExecutablePath { get; }

    public string? WindowTitleContains { get; }

    public string DisplayName
    {
        get
        {
            List<string> parts = [];
            if (ProcessName is not null)
            {
                parts.Add(ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                    ? ProcessName
                    : $"{ProcessName}.exe");
            }

            if (ExecutablePath is not null)
            {
                parts.Add(ExecutablePath);
            }

            if (WindowTitleContains is not null)
            {
                parts.Add($"title contains '{WindowTitleContains}'");
            }

            return string.Join(" / ", parts);
        }
    }

    public static ApplicationIdentity? FromTargetSelector(RuntimeTargetSelector selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return TryCreate(selector.ProcessName, selector.ExecutablePath, selector.WindowTitleContains);
    }

    public static ApplicationIdentity? FromTargetWindow(TargetWindowInfo? window)
    {
        return window is null
            ? null
            : TryCreate(window.ProcessName, window.ExecutablePath, window.Title);
    }

    public bool IsExactSameIdentity(ApplicationIdentity other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return string.Equals(ProcessName, other.ProcessName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ExecutablePath, other.ExecutablePath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(WindowTitleContains, other.WindowTitleContains, StringComparison.OrdinalIgnoreCase);
    }

    public bool Matches(ApplicationIdentity candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (ProcessName is not null)
        {
            if (!string.Equals(ProcessName, candidate.ProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (ExecutablePath is not null)
        {
            if (!string.Equals(ExecutablePath, candidate.ExecutablePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (WindowTitleContains is not null)
        {
            if (candidate.WindowTitleContains is null
                || !candidate.WindowTitleContains.Contains(WindowTitleContains, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public bool MatchesTargetIdentity(ApplicationIdentity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        bool matchedAnyConstraint = false;

        if (ProcessName is not null && target.ProcessName is not null)
        {
            if (!string.Equals(ProcessName, target.ProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            matchedAnyConstraint = true;
        }

        if (ExecutablePath is not null && target.ExecutablePath is not null)
        {
            if (!string.Equals(ExecutablePath, target.ExecutablePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            matchedAnyConstraint = true;
        }

        if (WindowTitleContains is not null && target.WindowTitleContains is not null)
        {
            if (!target.WindowTitleContains.Contains(WindowTitleContains, StringComparison.OrdinalIgnoreCase)
                && !WindowTitleContains.Contains(target.WindowTitleContains, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            matchedAnyConstraint = true;
        }

        return matchedAnyConstraint;
    }

    public static ApplicationIdentity? TryCreate(
        string? processName,
        string? executablePath = null,
        string? windowTitleContains = null)
    {
        try
        {
            return new ApplicationIdentity(processName, executablePath, windowTitleContains);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    internal static string? NormalizeProcessName(string? processName)
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

    internal static string? NormalizeExecutablePath(string? executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        string trimmed = executablePath.Trim();
        if (trimmed.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException($"Executable path '{trimmed}' contains invalid path characters.");
        }

        if (!Path.IsPathFullyQualified(trimmed))
        {
            throw new ArgumentException($"Executable path '{trimmed}' must be fully qualified.");
        }

        return Path.GetFullPath(trimmed);
    }
}
