namespace MouseShenanigans.Windows;

public interface IHotkeyBindingProvider
{
    IReadOnlyList<HotkeyBinding> GetBindings();
}
