namespace MouseShenanigans.Windows;

public static class DiagnosticEventTypes
{
    public const string ConfigurationLoadFallback = "configuration-load-fallback";
    public const string ConfigurationReloadFailed = "configuration-reload-failed";
    public const string ConfigurationSaveFailed = "configuration-save-failed";
    public const string DiagnosticsWriteFailed = "diagnostics-write-failed";
    public const string ForegroundCaptureAccepted = "foreground-capture-accepted";
    public const string ForegroundCaptureFailed = "foreground-capture-failed";
    public const string ForegroundCaptureRequested = "foreground-capture-requested";
    public const string ForegroundConfirmationAccepted = "foreground-confirmation-accepted";
    public const string ForegroundConfirmationCanceled = "foreground-confirmation-canceled";
    public const string ForegroundConfirmationRequested = "foreground-confirmation-requested";
    public const string LocalControlStarted = "local-control-started";
    public const string LocalControlStartupFailed = "local-control-startup-failed";
    public const string SafetyBlockedEnable = "safety-blocked-enable";
    public const string SelfExitRequested = "self-exit-requested";
    public const string StartupOverrideInvalid = "startup-override-invalid";
}
