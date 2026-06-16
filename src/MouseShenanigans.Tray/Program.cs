namespace MouseShenanigans.Tray;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        TrayStartupOptions startupOptions = TrayStartupOptions.FromEnvironment();

        if (!TraySingleInstanceGuard.TryAcquire(out TraySingleInstanceGuard? singleInstanceGuard))
        {
            return;
        }

        using (singleInstanceGuard)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext(startupOptions));
        }

        Environment.Exit(0);
    }
}
