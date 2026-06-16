namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal static class DesktopWindowController
{
    public static async Task FocusWindowAsync(string windowTitle, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IntPtr handle = NativeMethods.FindWindow(null, windowTitle);
            if (handle != IntPtr.Zero && NativeMethods.SetForegroundWindow(handle))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new TimeoutException($"Window '{windowTitle}' was not found or could not be focused within {timeout}.");
    }
}
