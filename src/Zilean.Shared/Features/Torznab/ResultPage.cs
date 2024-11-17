namespace Zilean.Shared.Features.Torznab;

public partial class ResultPage
{
    [GeneratedRegex(@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", RegexOptions.Compiled)]
    private static partial Regex InvalidXmlChars();
    private static XNamespace AtomNs => "http://www.w3.org/2005/Atom";
    private static XNamespace TorznabNs => "http://torznab.com/schemas/2015/feed";
    public IEnumerable<ReleaseInfo> Releases { get; set; } = [];
    private static string RemoveInvalidXmlChars(string? text) =>
        string.IsNullOrEmpty(text) ? null : InvalidXmlChars().Replace(text, "");

    private static string XmlDateFormat(DateTime dt)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        //Sat, 14 Mar 2015 17:10:42 -0400
        return $"{dt:ddd, dd MMM yyyy HH:mm:ss} " + $"{dt:zzz}".Replace(":", "");
    }

    private static XElement GetTorznabElement(string name, object? value) =>
        value is null ? null : new XElement(TorznabNs + "attr", new XAttribute("name", name), new XAttribute("value", value));

    public string ToXml(Uri selfAtom)
    {
        var xdoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", AtomNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "torznab", TorznabNs.NamespaceName),
                new XElement("channel",
                    new XElement(AtomNs + "link",
                        new XAttribute("href", selfAtom.AbsoluteUri),
                        new XAttribute("rel", "self"),
                        new XAttribute("type", "application/rss+xml")
                    ),
                    new XElement("title", ChannelInfo.Title),
                    new XElement("description", ChannelInfo.Description),
                    new XElement("link", ChannelInfo.Link.AbsoluteUri),
                    new XElement("language", ChannelInfo.Language),
                    new XElement("category", ChannelInfo.Category),
                    from r in Releases
                    select new XElement("item",
                        new XElement("title", RemoveInvalidXmlChars(r.Title)),
                        new XElement("guid", Parsing.CreateGuidFromInfohash(r.InfoHash)),
                        new XElement("type", ReleaseInfo.Origin),
                        r.Details == null ? null : new XElement("comments", r.Details.AbsoluteUri),
                        r.PublishDate == DateTime.MinValue ? new XElement("pubDate", XmlDateFormat(DateTime.Now)) : new XElement("pubDate", XmlDateFormat(r.PublishDate)),
                        r.Size == null ? null : new XElement("size", r.Size ?? 0),
                        new XElement("link", r.Magnet?.AbsoluteUri ?? string.Empty),
                        r.Category == null ? null : from c in r.Category select new XElement("category", c),
                        new XElement(
                            "enclosure",
                            new XAttribute("url", r.Magnet?.AbsoluteUri ?? string.Empty),
                            r.Size == null ? null : new XAttribute("length", r.Size),
                            new XAttribute("type", "application/x-bittorrent")
                        ),
                        r.Category == null ? null : from c in r.Category select GetTorznabElement("category", c),
                        GetTorznabElement("imdb", r.Imdb?.ToString("D7")),
                        GetTorznabElement("imdbid", r.Imdb != null ? "tt" + r.Imdb?.ToString("D7") : null),
                        r.Languages == null ? null : from c in r.Languages select GetTorznabElement("language", c),
                        GetTorznabElement("year", r.Year),
                        GetTorznabElement("seeders", ReleaseInfo.Seeders),
                        GetTorznabElement("peers", ReleaseInfo.Peers),
                        GetTorznabElement("infohash", RemoveInvalidXmlChars(r.InfoHash)),
                        GetTorznabElement("magneturl", r.Magnet?.AbsoluteUri)
                    )
                )
            )
        );

        return xdoc.Declaration + Environment.NewLine + xdoc;
    }
}
