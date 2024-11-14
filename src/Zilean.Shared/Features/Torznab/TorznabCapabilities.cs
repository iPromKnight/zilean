namespace Zilean.Shared.Features.Torznab;

public static class TorznabCapabilities
{
    public static List<TvSearch> TvSearchParams { get; } =
    [
        TvSearch.Q,
        TvSearch.Season,
        TvSearch.Ep,
        TvSearch.ImdbId,
        TvSearch.Year,
    ];

    public static List<MovieSearch> MovieSearchParams { get; } =
    [
        MovieSearch.Q,
        MovieSearch.ImdbId,
        MovieSearch.Year,
    ];

    public static int LimitsMax { get; } = 2000;
    public static int LimitsDefault { get; } = 100;
    public static bool SearchAvailable { get; } = true;
    public static bool SupportsRawSearch { get; } = true;
    public static bool TvSearchAvailable => TvSearchParams.Count > 0;
    public static bool TvSearchSeasonAvailable => TvSearchParams.Contains(TvSearch.Season);
    public static bool TvSearchEpAvailable => TvSearchParams.Contains(TvSearch.Ep);
    public static bool TvSearchImdbAvailable => TvSearchParams.Contains(TvSearch.ImdbId);
    public static bool TvSearchYearAvailable => TvSearchParams.Contains(TvSearch.Year);
    public static bool MovieSearchAvailable => MovieSearchParams.Count > 0;
    public static bool MovieSearchImdbAvailable => MovieSearchParams.Contains(MovieSearch.ImdbId);
    public static bool MovieSearchYearAvailable => MovieSearchParams.Contains(MovieSearch.Year);
    public static List<TorznabCategory> Categories { get; } =
    [
        TorznabCategoryTypes.Movies,
        TorznabCategoryTypes.TV,
    ];

    public static string ToXml() =>
        GetXDocument().Declaration + Environment.NewLine + GetXDocument();

    private static XDocument GetXDocument()
    {
        var xdoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("caps",
                new XElement("server",
                    new XAttribute("title", "Zilean")
                ),
                new XElement("limits",
                    new XAttribute("default", LimitsDefault),
                    new XAttribute("max", LimitsMax)
                ),
                new XElement("searching",
                    new XElement("search",
                        new XAttribute("available", SearchAvailable ? "yes" : "no"),
                        new XAttribute("supportedParams", "q"),
                        SupportsRawSearch ? new XAttribute("searchEngine", "raw") : null
                    ),
                    new XElement("tv-search",
                        new XAttribute("available", TvSearchAvailable ? "yes" : "no"),
                        new XAttribute("supportedParams", SupportedTvSearchParams()),
                        SupportsRawSearch ? new XAttribute("searchEngine", "raw") : null
                    ),
                    new XElement("movie-search",
                        new XAttribute("available", MovieSearchAvailable ? "yes" : "no"),
                        new XAttribute("supportedParams", SupportedMovieSearchParams()),
                        SupportsRawSearch ? new XAttribute("searchEngine", "raw") : null
                    )
                ),
                new XElement("categories",
                    from c in Categories.GetTorznabCategoryTree()
                    select new XElement("category",
                        new XAttribute("id", c.Id),
                        new XAttribute("name", c.Name),
                        from sc in c.SubCategories
                        select new XElement("subcat",
                            new XAttribute("id", sc.Id),
                            new XAttribute("name", sc.Name)
                        )
                    )
                )
            )
        );
        return xdoc;
    }

    private static string SupportedTvSearchParams()
    {
        var parameters = new List<string> { "q" };

        if (TvSearchSeasonAvailable)
        {
            parameters.Add("season");
        }

        if (TvSearchEpAvailable)
        {
            parameters.Add("ep");
        }

        if (TvSearchImdbAvailable)
        {
            parameters.Add("imdbid");
        }

        if (TvSearchYearAvailable)
        {
            parameters.Add("year");
        }

        return string.Join(",", parameters);
    }

    private static string SupportedMovieSearchParams()
    {
        var parameters = new List<string> { "q" };

        if (MovieSearchImdbAvailable)
        {
            parameters.Add("imdbid");
        }

        if (MovieSearchYearAvailable)
        {
            parameters.Add("year");
        }

        return string.Join(",", parameters);
    }
}
