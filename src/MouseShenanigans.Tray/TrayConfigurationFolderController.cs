using System.Diagnostics;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayConfigurationFolderController
{
    private readonly RuntimeConfigurationController configurationController;
    private readonly Action<string> openFolder;
    private readonly Action refreshStatus;

    public TrayConfigurationFolderController(
        RuntimeConfigurationController configurationController,
        Action<string>? openFolder,
        Action refreshStatus)
    {
        this.configurationController = configurationController ?? throw new ArgumentNullException(nameof(configurationController));
        this.openFolder = openFolder ?? OpenFolder;
        this.refreshStatus = refreshStatus ?? throw new ArgumentNullException(nameof(refreshStatus));
    }

    public void OpenConfigurationFolder()
    {
        try
        {
            string folderPath = GetConfigurationFolderPath(configurationController.ConfigurationPath);
            Directory.CreateDirectory(folderPath);
            openFolder(folderPath);
        }
        catch (Exception exception) when (exception is ArgumentException
            or IOException
            or InvalidOperationException
            or UnauthorizedAccessException
            or System.ComponentModel.Win32Exception)
        {
            configurationController.ReportOperationFailure(
                $"Configuration folder open failed: {exception.Message}");
        }

        refreshStatus();
    }

    public static string GetConfigurationFolderPath(string configurationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationPath);

        string? folderPath = Path.GetDirectoryName(configurationPath);
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException(
                "Configuration path does not include a directory.",
                nameof(configurationPath));
        }

        return folderPath;
    }

    private static void OpenFolder(string folderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true,
        });
    }
}
