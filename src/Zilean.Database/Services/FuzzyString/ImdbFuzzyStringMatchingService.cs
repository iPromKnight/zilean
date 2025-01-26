using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.PreProcess;
using Zilean.Shared.Extensions;

namespace Zilean.Database.Services.FuzzyString;

public class ImdbFuzzyStringMatchingService(ILogger<ImdbFuzzyStringMatchingService> logger, ZileanConfiguration configuration) : IImdbMatchingService
{
    private readonly ConcurrentDictionary<string, string?> _imdbCache = [];
    private ConcurrentDictionary<int,List<ImdbFile>>? _imdbTvFiles;
    private ConcurrentDictionary<int,List<ImdbFile>>? _imdbMovieFiles;
    private const double ExactMatchTitleYearScore = 2.0;
    private const double CloseMatchTitleYearScore = 1.5;

    public async Task PopulateImdbData()
    {
        _imdbTvFiles = await GetImdbTvFiles();
        _imdbMovieFiles = await GetImdbMovieFiles();
    }

    public void DisposeImdbData()
    {
        _imdbTvFiles.Clear();
        _imdbMovieFiles.Clear();

        _imdbTvFiles = null;
        _imdbMovieFiles = null;
    }

    public Task<ConcurrentQueue<TorrentInfo>> MatchImdbIdsForBatchAsync(IEnumerable<TorrentInfo> batch)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = configuration.Imdb.UseAllCores switch
            {
                true => Environment.ProcessorCount,
                false => configuration.Imdb.NumberOfCores,
            },
        };

        var updatedTorrents = new ConcurrentQueue<TorrentInfo>();

        Parallel.ForEach(
            batch, parallelOptions, (torrent, _) =>
            {
                if (_imdbCache.TryGetValue(torrent.CacheKey(), out var imdbId))
                {
                    torrent.ImdbId = imdbId;
                    return;
                }


                IEnumerable<ImdbFile> relevantImdbFiles;

                if (torrent.Year.HasValue)
                {
                    if (!HasFilteredPartitionsWithYear(_imdbTvFiles, _imdbMovieFiles, torrent, out var files))
                    {
                        return;
                    }

                    relevantImdbFiles = files;
                }
                else
                {
                    relevantImdbFiles = GetAllImdbFilesWithoutYear(_imdbTvFiles, _imdbMovieFiles, torrent);
                }

                var bestMatch = relevantImdbFiles
                    .Select(
                        imdb => new
                        {
                            imdb.ImdbId,
                            imdb.Title,
                            imdb.Year,
                            Score = CalculateScore(torrent, imdb),
                        })
                    .OrderByDescending(match => match.Score)
                    .FirstOrDefault();

                if (bestMatch == null)
                {
                    logger.LogWarning(
                        "No suitable match found for Torrent '{Title}', Category: {Category}",
                        torrent.ParsedTitle, torrent.Category);
                    return;
                }

                var bestMatchIsValid = bestMatch.Score >= configuration.Imdb.MinimumScoreMatch * 100;

                if (bestMatchIsValid && bestMatch.ImdbId != torrent.ImdbId)
                {
                    logger.LogInformation(
                        "Torrent '{Title}' updated from IMDb ID '{OldImdbId}' to '{NewImdbId}' with a score of {Score}, Category: {Category}, Imdb Title: {ImdbTitle}, Imdb Year: {ImdbYear}",
                        torrent.ParsedTitle, torrent.ImdbId, bestMatch.ImdbId, bestMatch.Score, torrent.Category, bestMatch.Title, bestMatch.Year);

                    torrent.ImdbId = bestMatch.ImdbId;

                    _imdbCache[torrent.CacheKey()] = bestMatch.ImdbId;

                    updatedTorrents.Enqueue(torrent);

                    return;
                }

                if (bestMatchIsValid)
                {
                    logger.LogInformation(
                        "Torrent '{Title}' retained its existing IMDb ID '{ImdbId}' with a best match score of {Score}, Category: {Category}, Imdb Title: {ImdbTitle}, Imdb Year: {ImdbYear}",
                        torrent.ParsedTitle, torrent.ImdbId, bestMatch.Score, torrent.Category, bestMatch.Title, bestMatch.Year);

                    return;
                }

                if (!bestMatchIsValid)
                {
                    logger.LogWarning(
                        "Best match for Torrent '{Title}' is '{ImdbId}' with a score of {Score}, Category: {Category}, Imdb Title: {ImdbTitle}, Imdb Year: {ImdbYear}, Below Minimum Score Cutoff : {MinimumScore}",
                        torrent.ParsedTitle, bestMatch.ImdbId, bestMatch.Score, torrent.Category, bestMatch.Title, bestMatch.Year, configuration.Imdb.MinimumScoreMatch * 100);
                }


                if (torrent.Year.HasValue)
                {
                    logger.LogWarning(
                        "No suitable match found for Torrent '{Title}' in year range {YearRange}, Category: {Category}",
                        torrent.ParsedTitle, $"{torrent.Year.Value - 1}-{torrent.Year.Value + 1}", torrent.Category);
                    return;
                }

                logger.LogWarning(
                    "No suitable match found for Torrent '{Title}', Category: {Category}",
                    torrent.ParsedTitle, torrent.Category);
            });

        return Task.FromResult(updatedTorrents);
    }

    private static double CalculateScore(TorrentInfo torrent, ImdbFile imdb) =>
        torrent.ParsedTitle == imdb.Title && torrent.Year == imdb.Year
            ? ExactMatchTitleYearScore * 100
            : torrent.ParsedTitle == imdb.Title && torrent.Year.HasValue &&
              Math.Abs(torrent.Year.Value - imdb.Year) <= 1
                ? CloseMatchTitleYearScore * 100
                : Fuzz.Ratio(torrent.ParsedTitle, imdb.Title, PreprocessMode.Full);

    private bool HasFilteredPartitionsWithYear(
        ConcurrentDictionary<int, List<ImdbFile>> imdbTvFilesByYear,
        ConcurrentDictionary<int, List<ImdbFile>> imdbMovieFilesByYear,
        TorrentInfo torrent,
        out IEnumerable<ImdbFile> relevantImdbFiles)
    {
        switch (torrent.Category)
        {
            case "tvSeries":
                relevantImdbFiles = GetFilteredFiles(imdbTvFilesByYear, torrent.Year!.Value, includeYearZero: true);
                break;

            case "movie":
                relevantImdbFiles = GetFilteredFiles(imdbMovieFilesByYear, torrent.Year!.Value, includeYearZero: true);
                break;

            default:
                logger.LogWarning("Torrent '{Title}' has an unknown category '{Category}', skipping", torrent.NormalizedTitle, torrent.Category);
                relevantImdbFiles = [];
                return false;
        }

        return true;
    }

    private static IEnumerable<ImdbFile> GetFilteredFiles(
        ConcurrentDictionary<int, List<ImdbFile>> filesByYear,
        int baseYear,
        bool includeYearZero)
    {
        var years = new[] { baseYear - 1, baseYear, baseYear + 1 };

        foreach (var year in years)
        {
            if (filesByYear.TryGetValue(year, out var files))
            {
                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }

        if (includeYearZero && filesByYear.TryGetValue(0, out var zeroYearFiles))
        {
            foreach (var file in zeroYearFiles)
            {
                yield return file;
            }
        }
    }

    private IEnumerable<ImdbFile> GetAllImdbFilesWithoutYear(ConcurrentDictionary<int, List<ImdbFile>> imdbTvFilesByYear, ConcurrentDictionary<int, List<ImdbFile>> imdbMovieFilesByYear, TorrentInfo torrent)
    {
        switch (torrent.Category)
        {
            case "tvSeries":
                return imdbTvFilesByYear.Values.SelectMany(files => files);
            case "movie":
                return imdbMovieFilesByYear.Values.SelectMany(files => files);
            default:
                logger.LogWarning("Torrent '{Title}' has an unknown category '{Category}', skipping", torrent.NormalizedTitle, torrent.Category);
                return [];
        }
    }

    private async Task<ConcurrentDictionary<int, List<ImdbFile>>> GetImdbMovieFiles()
    {
        logger.LogInformation("Loading all IMDB entries...");

        await using var sqlConnection = new NpgsqlConnection(configuration.Database.ConnectionString);
        await sqlConnection.OpenAsync();

        var imdbFiles = sqlConnection.Query<ImdbFile>(
            """
            SELECT "ImdbId", "Title", "Adult", "Category", "Year" FROM public."ImdbFiles"
            WHERE "Category" IN ('movie', 'tvMovie')
            """);

        var imdbFilesByYear = imdbFiles
            .GroupBy(imdb => imdb.Year)
            .ToConcurrentDictionary(g => g.Key, g => g.ToList());

        logger.LogInformation("Loaded {ImdbCount} IMDB entries, partitioned by {YearCount} years", imdbFilesByYear.Values.Sum(x => x.Count), imdbFilesByYear.Count);

        return imdbFilesByYear;
    }

    private async Task<ConcurrentDictionary<int, List<ImdbFile>>> GetImdbTvFiles()
    {
        logger.LogInformation("Loading all IMDB entries...");

        await using var sqlConnection = new NpgsqlConnection(configuration.Database.ConnectionString);
        await sqlConnection.OpenAsync();

        var imdbFiles = sqlConnection.Query<ImdbFile>(
            """
            SELECT "ImdbId", "Title", "Adult", "Category", "Year" FROM public."ImdbFiles"
            WHERE "Category" IN ('tvSeries', 'tvShort', 'tvMiniSeries', 'tvSpecial')
            """);

        var imdbFilesByYear = imdbFiles
            .GroupBy(imdb => imdb.Year)
            .ToConcurrentDictionary(g => g.Key, g => g.ToList());

        logger.LogInformation("Loaded {ImdbCount} IMDB entries, partitioned by {YearCount} years", imdbFilesByYear.Values.Sum(x => x.Count), imdbFilesByYear.Count);

        return imdbFilesByYear;
    }
}
