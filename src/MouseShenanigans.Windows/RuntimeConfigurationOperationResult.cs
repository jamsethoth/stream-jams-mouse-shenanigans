namespace MouseShenanigans.Windows;

public sealed record RuntimeConfigurationOperationResult(
    RuntimeConfiguration Configuration,
    bool Succeeded,
    string? Message);
