namespace Zilean.Shared.Features.Torznab;

public partial class TorznabQuery
{
    [GeneratedRegex(@"\p{Pd}+", RegexOptions.Compiled)]
    private static partial Regex StandardizeDashesRegex();

    [GeneratedRegex(@"[\u0060\u00B4\u2018\u2019]", RegexOptions.Compiled)]
    private static partial Regex StandardizeSingleQuotesRegex();
    [GeneratedRegex("[^\\w]+")]
    private static partial Regex SplitRegex();
    public bool InteractiveSearch { get; set; }
    public string? QueryType { get; set; }
    public int[] Categories { get; set; } = [];
    public int Extended { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string? ImdbID { get; set; }
    public string[]? QueryStringParts { get; set; }
    public int? Season { get; set; }
    public int? Episode { get; set; }
    public string? SearchTerm { get; set; }
    public int? Year { get; set; }
    public bool IsTest { get; set; } = false;

    public string ImdbIDShort => ImdbID?.TrimStart('t');

    public bool IsSearch => QueryType == "search";

    public bool IsTVSearch => QueryType == "tvsearch";
    public bool IsXxxSearch => QueryType == "xxx";

    public bool IsMovieSearch => QueryType == "movie";
    public bool IsImdbQuery => ImdbID != null;

    public bool IsRssSearch =>
        string.IsNullOrWhiteSpace(SearchTerm) &&
        !IsIdSearch;

    public bool IsIdSearch =>
        Episode.GetValueOrDefault() > 0 ||
        Season.GetValueOrDefault() > 0 ||
        IsImdbQuery ||
        Year.GetValueOrDefault() > 0;

    public bool HasSpecifiedCategories => Categories is { Length: > 0 };

    public string SanitizedSearchTerm
    {
        get
        {
            var term = SearchTerm ?? "";

            term = StandardizeDashesRegex().Replace(term, "-");
            term = StandardizeSingleQuotesRegex().Replace(term, "'");

            var safeTitle = term.Where(c => char.IsLetterOrDigit(c)
                                             || char.IsWhiteSpace(c)
                                             || c == '-'
                                             || c == '.'
                                             || c == '_'
                                             || c == '('
                                             || c == ')'
                                             || c == '@'
                                             || c == '/'
                                             || c == '\''
                                             || c == '['
                                             || c == ']'
                                             || c == '+'
                                             || c == '%'
                                             || c == ':'
                                           );

            return string.Concat(safeTitle);
        }
    }

    public TorznabQuery CreateFallback(string? search)
    {
        var ret = Clone();
        if (Categories.Length == 0)
        {
            ret.Categories =
            [
                TorznabCategoryTypes.Movies.Id,
                TorznabCategoryTypes.MoviesForeign.Id,
                TorznabCategoryTypes.MoviesOther.Id,
                TorznabCategoryTypes.MoviesSD.Id,
                TorznabCategoryTypes.MoviesHD.Id,
                TorznabCategoryTypes.Movies3D.Id,
                TorznabCategoryTypes.MoviesBluRay.Id,
                TorznabCategoryTypes.MoviesDVD.Id,
                TorznabCategoryTypes.MoviesWEBDL.Id,
                TorznabCategoryTypes.MoviesUHD.Id
            ];
        }
        ret.SearchTerm = search;

        return ret;
    }

    public TorznabQuery Clone()
    {
        var ret = new TorznabQuery
        {
            InteractiveSearch = InteractiveSearch,
            QueryType = QueryType,
            Extended = Extended,
            Limit = Limit,
            Offset = Offset,
            Season = Season,
            Episode = Episode,
            SearchTerm = SearchTerm,
            IsTest = IsTest,
            Year = Year,
            ImdbID = ImdbID,
        };

        if (Categories.Length > 0)
        {
            ret.Categories = new int[Categories.Length];
            Array.Copy(Categories, ret.Categories, Categories.Length);
        }

        if (QueryStringParts?.Length > 0)
        {
            ret.QueryStringParts = new string[QueryStringParts.Length];
            Array.Copy(QueryStringParts, ret.QueryStringParts, QueryStringParts.Length);
        }

        return ret;
    }

    public string GetQueryString() => (SanitizedSearchTerm + " " + GetEpisodeSearchString()).Trim();

    public bool MatchQueryStringAnd(string title, int? limit = null, string? queryStringOverride = null)
    {
        var commonWords = new[] { "and", "the", "an" };

        if (QueryStringParts == null)
        {
            var queryString = !string.IsNullOrWhiteSpace(queryStringOverride) ? queryStringOverride : GetQueryString();

            if (limit is > 0)
            {
                if (limit > queryString.Length)
                {
                    limit = queryString.Length;
                }

                queryString = queryString[..(int)limit];
            }

            QueryStringParts = [.. SplitRegex().Split(queryString).Where(p => !string.IsNullOrWhiteSpace(p) && p.Length > 1 && !commonWords.ContainsIgnoreCase(p))];
        }

        // Check if each part of the query string is in the given title.
        return QueryStringParts.All(title.ContainsIgnoreCase);
    }

    public string GetEpisodeSearchString()
    {
        if (Season is null or 0)
        {
            return string.Empty;
        }

        string episodeString;
        if (DateTime.TryParseExact($"{Season} {Episode}", "yyyy MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var showDate))
        {
            episodeString = showDate.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);
        }
        else if (!Episode.HasValue)
        {
            episodeString = $"S{Season:00}";
        }
        else
        {
            try
            {
                episodeString = $"S{Season:00}E{Parsing.CoerceInt(Episode.GetValueOrDefault().ToString()):00}";
            }
            catch (FormatException) // e.g. seaching for S01E01A
            {
                episodeString = $"S{Season:00}E{Episode}";
            }
        }

        return episodeString;
    }
}
