namespace Zilean.Database.Services;

public class TorrentInfoService(ILogger<TorrentInfoService> logger, ZileanConfiguration configuration, IServiceProvider serviceProvider)
    : BaseDapperService(logger, configuration), ITorrentInfoService
{
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
        var imdbMatchingService = serviceScope.ServiceProvider.GetRequiredService<IImdbMatchingService>();

        await imdbMatchingService.PopulateImdbData();

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
                await imdbMatchingService.MatchImdbIdsForBatchAsync(batch);
            }

            logger.LogInformation("Storing batch {CurrentBatch} of {TotalBatches}", currentBatch, chunks.Count);
            await dbContext.BulkInsertOrUpdateAsync(batch, bulkConfig);
        }

        imdbMatchingService.DisposeImdbData();
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
}
