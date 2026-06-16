using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray;

public sealed class TrayForegroundAllowlistConfirmationPresenter
{
    private readonly ForegroundAllowlistConfirmationController confirmationController;
    private readonly Action refreshStatus;

    public TrayForegroundAllowlistConfirmationPresenter(
        ForegroundAllowlistConfirmationController confirmationController,
        Action refreshStatus)
    {
        this.confirmationController = confirmationController ?? throw new ArgumentNullException(nameof(confirmationController));
        this.refreshStatus = refreshStatus ?? throw new ArgumentNullException(nameof(refreshStatus));
    }

    public void ShowConfirmation(ForegroundAllowlistConfirmationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        DialogResult result = MessageBox.Show(
            CreatePromptText(request.Identity),
            "Allow Mouse Shenanigans target",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes)
        {
            confirmationController.Confirm(request.Id);
        }
        else
        {
            confirmationController.Cancel(request.Id);
        }

        refreshStatus();
    }

    private static string CreatePromptText(MouseShenanigans.Windows.ApplicationIdentity identity)
    {
        return string.Join(
            Environment.NewLine,
            "Add this foreground application to allowlistedGames?",
            string.Empty,
            identity.DisplayName,
            string.Empty,
            "Runtime remapping will stay disabled until you enable it.");
    }
}
