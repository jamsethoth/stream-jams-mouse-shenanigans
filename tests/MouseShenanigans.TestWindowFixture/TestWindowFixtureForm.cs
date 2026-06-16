namespace MouseShenanigans.TestWindowFixture;

public sealed class TestWindowFixtureForm : Form
{
    private readonly TestWindowFixtureOptions options;
    private bool readinessSignaled;

    public TestWindowFixtureForm(TestWindowFixtureOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        Text = options.WindowTitle;
        Name = "MouseShenanigansTestWindowFixture";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 640;
        Height = 360;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        SignalReady();
    }

    public void SignalReady()
    {
        if (readinessSignaled)
        {
            return;
        }

        readinessSignaled = true;
        TestWindowFixtureReadinessSignal.Write(options.ReadyFilePath, Text);
    }
}
