using System.ComponentModel;
using System.Diagnostics;

namespace MouseShenanigans.Windows;

public sealed class ProcessSnapshotReader : IProcessSnapshotReader
{
    public IReadOnlyList<ProcessSnapshot> ReadProcesses()
    {
        List<ProcessSnapshot> snapshots = [];
        foreach (Process process in Process.GetProcesses())
        {
            using (process)
            {
                ApplicationIdentity? identity = ApplicationIdentity.TryCreate(
                    process.ProcessName,
                    TryReadExecutablePath(process));
                if (identity is not null)
                {
                    snapshots.Add(new ProcessSnapshot(process.Id, identity));
                }
            }
        }

        return snapshots;
    }

    private static string? TryReadExecutablePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or NotSupportedException)
        {
            return null;
        }
    }
}
