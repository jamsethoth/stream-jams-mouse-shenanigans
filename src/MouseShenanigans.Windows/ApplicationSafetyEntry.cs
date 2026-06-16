namespace MouseShenanigans.Windows;

public sealed record ApplicationSafetyEntry
{
    public ApplicationSafetyEntry(ApplicationIdentity identity, string? label = null)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
    }

    public ApplicationIdentity Identity { get; }

    public string? Label { get; }

    public string DisplayName => Label is null
        ? Identity.DisplayName
        : $"{Label} ({Identity.DisplayName})";
}
