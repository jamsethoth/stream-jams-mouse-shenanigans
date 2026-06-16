namespace MouseShenanigans.TestWindowFixture;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        TestWindowFixtureOptions options;
        try
        {
            options = TestWindowFixtureOptions.Parse(args);
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(
                exception.Message,
                TestWindowFixtureOptions.DefaultWindowTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 2;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TestWindowFixtureForm(options));
        return 0;
    }
}
