namespace MouseShenanigans.TestWindowFixture;

public sealed record TestWindowFixtureOptions(string WindowTitle, string? ReadyFilePath)
{
    public const string DefaultWindowTitle = "MouseShenanigans Test Window Fixture";

    public static TestWindowFixtureOptions Default { get; } = new(DefaultWindowTitle, ReadyFilePath: null);

    public static TestWindowFixtureOptions Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string windowTitle = DefaultWindowTitle;
        string? readyFilePath = null;

        for (int index = 0; index < args.Count; index++)
        {
            string arg = args[index];
            switch (arg)
            {
                case "--title":
                    windowTitle = ReadRequiredValue(args, ref index, arg);
                    break;
                case "--ready-file":
                    readyFilePath = ReadRequiredValue(args, ref index, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown fixture argument '{arg}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(windowTitle))
        {
            throw new ArgumentException("Fixture window title must not be empty.");
        }

        return new TestWindowFixtureOptions(windowTitle.Trim(), readyFilePath);
    }

    private static string ReadRequiredValue(IReadOnlyList<string> args, ref int index, string optionName)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"{optionName} requires a value.");
        }

        index++;
        string value = args[index];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{optionName} requires a non-empty value.");
        }

        return value;
    }
}
