using System.Text;

namespace MouseShenanigans.TestWindowFixture;

public static class TestWindowFixtureReadinessSignal
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void Write(string? readyFilePath, string windowTitle)
    {
        if (string.IsNullOrWhiteSpace(readyFilePath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(readyFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            readyFilePath,
            $"ready=true\nwindowTitle={windowTitle}\nprocessName={Path.GetFileNameWithoutExtension(Environment.ProcessPath)}\n",
            Utf8NoBom);
    }
}
