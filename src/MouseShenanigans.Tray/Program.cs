namespace MouseShenanigans.Tray;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!TraySingleInstanceGuard.TryAcquire(out TraySingleInstanceGuard? singleInstanceGuard))
        {
            return;
        }

        using (singleInstanceGuard)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
    }
}
