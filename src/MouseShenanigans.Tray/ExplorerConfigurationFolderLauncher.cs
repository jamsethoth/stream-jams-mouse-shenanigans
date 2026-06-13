using System.Diagnostics;

namespace MouseShenanigans.Tray;

public sealed class ExplorerConfigurationFolderLauncher : IConfigurationFolderLauncher
{
    public void Open(string folderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = QuoteArgument(folderPath),
            UseShellExecute = false,
        });
    }

    private static string QuoteArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }
}
