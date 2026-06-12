using MouseShenanigans.Tray;

namespace MouseShenanigans.Tray.Tests;

public sealed class TrayHotkeyReceiverTests
{
    private const int WmHotkey = 0x0312;

    [Fact]
    public void TryDispatchMessageDispatchesHotkeyMessages()
    {
        var dispatchedIds = new List<int>();
        using var receiver = new TrayHotkeyReceiver(id =>
        {
            dispatchedIds.Add(id);
            return true;
        });

        bool handled = receiver.TryDispatchMessage(WmHotkey, new IntPtr(42));

        Assert.True(handled);
        Assert.Equal([42], dispatchedIds);
    }

    [Fact]
    public void TryDispatchMessageIgnoresNonHotkeyMessages()
    {
        var dispatchRequests = 0;
        using var receiver = new TrayHotkeyReceiver(_ =>
        {
            dispatchRequests++;
            return true;
        });

        bool handled = receiver.TryDispatchMessage(0x0100, IntPtr.Zero);

        Assert.False(handled);
        Assert.Equal(0, dispatchRequests);
    }

    [Fact]
    public void TryDispatchMessageLeavesUnknownHotkeysUnhandled()
    {
        using var receiver = new TrayHotkeyReceiver(_ => false);

        bool handled = receiver.TryDispatchMessage(WmHotkey, new IntPtr(99));

        Assert.False(handled);
    }
}
