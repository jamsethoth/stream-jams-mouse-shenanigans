namespace MouseShenanigans.Windows;

public sealed record ProcessSnapshot(int ProcessId, ApplicationIdentity? Identity);
