namespace MouseShenanigans.Windows;

public sealed class RuntimeConfigurationPathProvider : IRuntimeConfigurationPathProvider
{
    private readonly string? overridePath;

    public RuntimeConfigurationPathProvider(string? overridePath = null)
    {
        this.overridePath = string.IsNullOrWhiteSpace(overridePath) ? null : overridePath;
    }

    public string GetConfigurationPath()
    {
        if (overridePath is not null)
        {
            return overridePath;
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StreamJams",
            "MouseShenanigans",
            "config.json");
    }
}
