using MouseShenanigans.Tray;
using MouseShenanigans.Windows;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayStatusFormatterTests
{
    [Fact]
    public void RuntimeStatusTextIncludesSafetyAndSelfExitMessages()
    {
        string text = TrayStatusFormatter.CreateRuntimeStatusText(
            RuntimeRemappingStatus.Disabled,
            RuntimeProofOfConceptDefaults.CreateConfiguration(),
            applicationSafetyStatus: "Application safety blocked enable: target is not allowlisted.",
            selfExitStatus: "Self-exit requested because running process matched.");

        Assert.Contains("Application safety blocked enable", text, StringComparison.Ordinal);
        Assert.Contains("Self-exit requested", text, StringComparison.Ordinal);
    }
}
