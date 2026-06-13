using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayProfileMenuController
{
    private readonly ToolStripMenuItem profileMenuItem;
    private readonly RuntimeCommandController commandController;
    private readonly Action refreshStatus;

    public TrayProfileMenuController(
        ToolStripMenuItem profileMenuItem,
        RuntimeCommandController commandController,
        Action refreshStatus)
    {
        this.profileMenuItem = profileMenuItem ?? throw new ArgumentNullException(nameof(profileMenuItem));
        this.commandController = commandController ?? throw new ArgumentNullException(nameof(commandController));
        this.refreshStatus = refreshStatus ?? throw new ArgumentNullException(nameof(refreshStatus));
    }

    public void RefreshProfiles()
    {
        profileMenuItem.DropDownItems.Clear();

        RuntimeConfiguration? configuration = commandController.CurrentConfiguration;
        if (configuration is null)
        {
            profileMenuItem.Enabled = false;
            return;
        }

        profileMenuItem.Enabled = true;
        foreach (string profileName in configuration.ProfileNames)
        {
            var item = new ToolStripMenuItem(profileName)
            {
                Checked = string.Equals(
                    profileName,
                    configuration.ActiveProfileName,
                    StringComparison.OrdinalIgnoreCase),
            };

            item.Click += (_, _) =>
            {
                commandController.SelectProfile(profileName);
                RefreshProfiles();
                refreshStatus();
            };

            profileMenuItem.DropDownItems.Add(item);
        }
    }

    public void ReloadConfiguration()
    {
        commandController.ReloadConfiguration();
        RefreshProfiles();
        refreshStatus();
    }
}
