using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.PreProcess;

namespace Zilean.Database.Services;

public class TorrentInfoService(ILogger<TorrentInfoService> logger, ZileanConfiguration configuration, IServiceProvider serviceProvider)
    : BaseDapperService(logger, configuration), ITorrentInfoService
{
    private readonly ConcurrentDictionary<string, string?> _imdbCache = [];
    private const double ExactMatchTitleYearScore = 2.0;
    private const double CloseMatchTitleYearScore = 1.5;

    public async Task VaccumTorrentsIndexes(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync("VACUUM (VERBOSE, ANALYZE) \"Torrents\"", cancellationToken: cancellationToken);
    }

    public async Task StoreTorrentInfo(List<TorrentInfo> torrents, int batchSize = 5000)
    {
        if (torrents.Count == 0)
        {
            logger.LogInformation("No torrents to store.");
            return;
        }

        foreach (var torrentInfo in torrents)
        {
            torrentInfo.CleanedParsedTitle = Parsing.CleanQuery(torrentInfo.ParsedTitle);
        }

        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await using var connection = new NpgsqlConnection(Configuration.Database.ConnectionString);
        var imdbService = serviceScope.ServiceProvider.GetRequiredService<IImdbFileService>();

        var imdbTvFilesByYear = await imdbService.GetImdbTvFiles();
        var imdbMovieFilesByYear = await imdbService.GetImdbMovieFiles();

        var bulkConfig = new BulkConfig
        {
            SetOutputIdentity = false,
            BatchSize = batchSize,
            PropertiesToIncludeOnUpdate = [string.Empty],
            UpdateByProperties = ["InfoHash"],
            BulkCopyTimeout = 0,
            TrackingEntities = false,
        };

        dbContext.Database.SetCommandTimeout(0);

        var chunks = torrents.Chunk(batchSize).ToList();

        logger.LogInformation("Storing {Count} torrents in {BatchSize} batches", torrents.Count, chunks.Count);
        var currentBatch = 0;
        foreach (var batch in chunks)
        {
            currentBatch++;

            if (Configuration.Imdb.EnableImportMatching)
            {
                logger.LogInformation("Matching IMDb IDs for batch {CurrentBatch} of {TotalBatches}", currentBatch, chunks.Count);
                await MatchImdbIdsForBatchAsync(batch, imdbTvFilesByYear, imdbMovieFilesByYear);
            }

            logger.LogInformation("Storing batch {CurrentBatch} of {TotalBatches}", currentBatch, chunks.Count);
            await dbContext.BulkInsertOrUpdateAsync(batch, bulkConfig);
        }
    }

    public async Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query)
    {
        var cleanQuery = Parsing.CleanQuery(query);

        return await ExecuteCommandAsync(async connection =>
        {
            var sql =
                """
                SELECT
                    *
                FROM "Torrents"
                WHERE "ParsedTitle" % @query
                AND Length("InfoHash") = 40
                LIMIT 100;
                """;

            var parameters = new DynamicParameters();

            parameters.Add("@query", cleanQuery);

            var result = await connection.QueryAsync<TorrentInfo>(sql, parameters);

            return result.ToArray();
        }, "Error finding unfiltered dmm entries.");
    }

    public async Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter, int? limit = null)
    {
        var cleanQuery = Parsing.CleanQuery(filter.Query);
        var imdbId = EnsureCorrectFormatImdbId(filter);

        return await ExecuteCommandAsync(async connection =>
        {
            const string sql =
                """
                   SELECT *
                   FROM search_torrents_meta(
                       @Query,
                       @Season,
                       @Episode,
                       @Year,
                       @Language,
                       @Resolution,
                       @ImdbId,
                       @Limit,
                       @Category,
                       @SimilarityThreshold
                   );
                """;

            var parameters = new DynamicParameters();

            parameters.Add("@Query", cleanQuery);
            parameters.Add("@Season", filter.Season);
            parameters.Add("@Episode", filter.Episode);
            parameters.Add("@Year", filter.Year);
            parameters.Add("@Language", filter.Language);
            parameters.Add("@Resolution", filter.Resolution);
            parameters.Add("@Category", filter.Category);
            parameters.Add("@ImdbId", imdbId);
            parameters.Add("@Limit", limit ?? Configuration.Dmm.MaxFilteredResults);
            parameters.Add("@SimilarityThreshold", (float)Configuration.Dmm.MinimumScoreMatch);

            var results = await connection.QueryAsync<TorrentInfoResult>(sql, parameters);

            // assign imdb to torrent info
            return results.Select(MapImdbDataToTorrentInfo).ToArray();
        }, "Error finding unfiltered dmm entries.");
    }

    private static string? EnsureCorrectFormatImdbId(TorrentInfoFilter filter)
    {
        string? imdbId = null;
        if (!string.IsNullOrEmpty(filter.ImdbId))
        {
            imdbId = filter.ImdbId.StartsWith("tt") ? filter.ImdbId : $"tt{filter.ImdbId}";
        }

        return imdbId;
    }

    private static Func<TorrentInfoResult, TorrentInfoResult> MapImdbDataToTorrentInfo =>
        torrentInfo =>
        {
            if (torrentInfo.ImdbId != null)
            {
                torrentInfo.Imdb = new()
                {
                    ImdbId = torrentInfo.ImdbId,
                    Category = torrentInfo.ImdbCategory,
                    Title = torrentInfo.ImdbTitle,
                    Year = torrentInfo.ImdbYear ?? 0,
                    Adult = torrentInfo.ImdbAdult,
                };
            }

            return torrentInfo;
        };

    public async Task<ConcurrentBag<TorrentInfo>> MatchImdbIdsForBatchAsync(
        IEnumerable<TorrentInfo> batch,
        ConcurrentDictionary<int, List<ImdbFile>> imdbTvFilesByYear,
        ConcurrentDictionary<int, List<ImdbFile>> imdbMovieFilesByYear)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Configuration.Imdb.UseAllCores switch
            {
                true => Environment.ProcessorCount,
                false => Configuration.Imdb.NumberOfCores,
            },
        };

        var updatedTorrents = new ConcurrentBag<TorrentInfo>();

        await Parallel.ForEachAsync(
            batch, parallelOptions, async (torrent, _) =>
            {
                if (_imdbCache.TryGetValue(torrent.CacheKey(), out var imdbId))
                {
                    torrent.ImdbId = imdbId;
                    return;
                }

                if (!torrent.Year.HasValue)
                {
                    logger.LogWarning("Torrent '{Title}' has no year information, skipping", torrent.NormalizedTitle);
                    return;
                }

                List<ImdbFile> relevantImdbFiles;

                switch (torrent.Category)
                {
                    case "tvSeries":
                        relevantImdbFiles = Enumerable.Range(torrent.Year.Value - 1, 3)
                            .Where(year => imdbTvFilesByYear.TryGetValue(year, out var _))
                            .SelectMany(year => imdbTvFilesByYear[year])
                            .ToList();
                        break;
                    case "movie":
                        relevantImdbFiles = Enumerable.Range(torrent.Year.Value - 1, 3)
                            .Where(year => imdbMovieFilesByYear.TryGetValue(year, out var _))
                            .SelectMany(year => imdbMovieFilesByYear[year])
                            .ToList();
                        break;
                    default:
                        logger.LogWarning("Torrent '{Title}' has an unknown category '{Category}', skipping", torrent.NormalizedTitle, torrent.Category);
                        return;
                }

                if (relevantImdbFiles.Count == 0)
                {
                    logger.LogWarning(
                        "No IMDb entries found for Torrent '{Title}' in year range {YearRange}. Skipping",
                        torrent.NormalizedTitle, $"{torrent.Year.Value - 1}-{torrent.Year.Value + 1}");
                    return;
                }

                var bestMatch = relevantImdbFiles
                    .Select(
                        imdb => new
                        {
                            imdb.ImdbId,
                            Score = CalculateScore(torrent, imdb),
                        })
                    .Where(match => match.Score >= Configuration.Imdb.MinimumScoreMatch * 100)
                    .OrderByDescending(match => match.Score)
                    .FirstOrDefault();

                if (bestMatch != null && bestMatch.ImdbId != torrent.ImdbId)
                {
                    logger.LogInformation(
                        "Torrent '{Title}' updated from IMDb ID '{OldImdbId}' to '{NewImdbId}' with a score of {Score}",
                        torrent.NormalizedTitle, torrent.ImdbId, bestMatch.ImdbId, bestMatch.Score);

                    torrent.ImdbId = bestMatch.ImdbId;

                    _imdbCache[torrent.CacheKey()] = bestMatch.ImdbId;

                    updatedTorrents.Add(torrent);
                }
                else if (bestMatch != null)
                {
                    logger.LogInformation(
                        "Torrent '{Title}' retained its existing IMDb ID '{ImdbId}' with a best match score of {Score}",
                        torrent.NormalizedTitle, torrent.ImdbId, bestMatch.Score);
                }
                else
                {
                    logger.LogWarning(
                        "No suitable match found for Torrent '{Title}' in year range {YearRange}",
                        torrent.NormalizedTitle, $"{torrent.Year.Value - 1}-{torrent.Year.Value + 1}");
                }

                await Task.CompletedTask;
            });

        return updatedTorrents;
    }

    public async Task<HashSet<string>> GetExistingInfoHashesAsync(List<string> infoHashes)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var existingHashes = await dbContext.Torrents
            .Where(t => infoHashes.Contains(t.InfoHash))
            .Select(t => t.InfoHash)
            .ToListAsync();

        return [..existingHashes];
    }

    public async Task<HashSet<string>> GetBlacklistedItems()
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var existingHashes = await dbContext.BlacklistedItems
            .Select(t => t.InfoHash)
            .ToListAsync();

        return [..existingHashes];
    }

    private static double CalculateScore(TorrentInfo torrent, ImdbFile imdb) =>
        torrent.NormalizedTitle == imdb.Title && torrent.Year == imdb.Year
            ? ExactMatchTitleYearScore * 100
            : torrent.NormalizedTitle == imdb.Title && torrent.Year.HasValue &&
              Math.Abs(torrent.Year.Value - imdb.Year) <= 1
                ? CloseMatchTitleYearScore * 100
                : Fuzz.Ratio(torrent.NormalizedTitle, imdb.Title, PreprocessMode.None);
}
