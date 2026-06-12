namespace MouseShenanigans.Windows;

public sealed class RuntimeConfigurationPathProvider : IRuntimeConfigurationPathProvider
{
    public string GetConfigurationPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StreamJams",
            "MouseShenanigans",
            "config.json");
    }
}
