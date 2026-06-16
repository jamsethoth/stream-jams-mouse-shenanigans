namespace MouseShenanigans.Windows;

public enum ForegroundAllowlistConfirmationSource
{
    Hotkey,
    LocalControl,
}

public enum ForegroundAllowlistConfirmationStatus
{
    Pending,
    Accepted,
    Canceled,
    AlreadyAllowed,
}

public sealed record ForegroundAllowlistConfirmationRequest(
    Guid Id,
    ApplicationIdentity Identity,
    ForegroundAllowlistConfirmationSource Source,
    ForegroundAllowlistConfirmationStatus Status);

public sealed record ForegroundAllowlistConfirmationRequestResult(
    bool Succeeded,
    ForegroundAllowlistConfirmationRequest? Request,
    string Message);

public sealed record ForegroundAllowlistConfirmationCompletionResult(
    bool Succeeded,
    ForegroundAllowlistConfirmationRequest? Request,
    string Message);
