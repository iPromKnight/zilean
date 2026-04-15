using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.PreProcess;

namespace Zilean.Database.Services;

public static class ImdbTitleMatching
{
    private const double ExactMatchTitleYearScore = 2.0;
    private const double CloseMatchTitleYearScore = 1.5;

    public static double CalculateScore(TorrentInfo torrent, ImdbFile imdb) =>
        GetCandidateTitles(imdb)
            .Select(title => CalculateScore(torrent.ParsedTitle, torrent.Year, title, imdb.Year))
            .DefaultIfEmpty(0)
            .Max();

    public static IReadOnlyCollection<string> GetCandidateTitles(ImdbFile imdb) =>
        [..
            new[] { imdb.Title, imdb.OriginalTitle }
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Select(title => title!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];

    private static double CalculateScore(string? torrentTitle, int? torrentYear, string imdbTitle, int imdbYear)
    {
        if (string.IsNullOrWhiteSpace(torrentTitle))
        {
            return 0;
        }

        return torrentTitle == imdbTitle && torrentYear == imdbYear
            ? ExactMatchTitleYearScore * 100
            : torrentTitle == imdbTitle && torrentYear.HasValue && Math.Abs(torrentYear.Value - imdbYear) <= 1
                ? CloseMatchTitleYearScore * 100
                : Fuzz.Ratio(torrentTitle, imdbTitle, PreprocessMode.Full);
    }
}
