namespace MouseShenanigans.Windows;

public interface IProcessSnapshotReader
{
    IReadOnlyList<ProcessSnapshot> ReadProcesses();
}
