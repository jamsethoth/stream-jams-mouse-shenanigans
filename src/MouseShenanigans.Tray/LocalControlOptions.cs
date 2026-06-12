namespace MouseShenanigans.Tray;

public sealed record LocalControlOptions
{
    public const string DefaultUrl = "http://127.0.0.1:5178";

    private LocalControlOptions(Uri url)
    {
        Url = url;
    }

    public Uri Url { get; }

    public string UrlText => Url.GetLeftPart(UriPartial.Authority);

    public static LocalControlOptions Default { get; } = Create(DefaultUrl);

    public static LocalControlOptions Create(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("Local control URL must be an absolute HTTP URL.", nameof(url));
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Local control URL must use HTTP.", nameof(url));
        }

        if (!uri.IsLoopback)
        {
            throw new ArgumentException("Local control URL must bind to a loopback address.", nameof(url));
        }

        return new LocalControlOptions(uri);
    }
}
