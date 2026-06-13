namespace MouseShenanigans.Windows;

public sealed record RuntimeConfigurationLoadResult(
    RuntimeConfiguration Configuration,
    bool UsedFallback,
    string? ErrorMessage);
