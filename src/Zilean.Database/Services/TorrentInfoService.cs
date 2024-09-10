namespace Zilean.Database.Services;

public class TorrentInfoService(ILogger<TorrentInfoService> logger, ZileanConfiguration configuration, IServiceProvider serviceProvider)
    : BaseDapperService(logger, configuration), ITorrentInfoService
{
    private readonly ConcurrentDictionary<string, string?> _imdbCache = [];

    public async Task StoreTorrentInfo(List<TorrentInfo> torrents, int batchSize = 5000)
    {
        if (torrents.Count == 0)
        {
            logger.LogInformation("No torrents to store.");
            return;
        }

        logger.LogInformation("Storing {Count} torrents", torrents.Count);

        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await using var connection = new NpgsqlConnection(Configuration.Database.ConnectionString);

        var bulkConfig = new BulkConfig
        {
            SetOutputIdentity = false,
            BatchSize = batchSize,
            PropertiesToIncludeOnUpdate = [string.Empty],
            UpdateByProperties = ["InfoHash"],
            BulkCopyTimeout = 0,
            NotifyAfter = 250,
            TrackingEntities = false,
        };

        dbContext.Database.SetCommandTimeout(0);

        var chunks = torrents.Chunk(batchSize).ToList();

        logger.LogInformation("Storing {Count} torrents in {BatchSize} batches", torrents.Count, chunks.Count);
        var currentBatch = 0;
        foreach (var batch in chunks)
        {
            currentBatch++;

            bulkConfig.NotifyAfter = (int)Math.Ceiling(batch.Length * 0.05);

            if (Configuration.Imdb.EnableImportMatching)
            {
                logger.LogInformation("Fetching IMDb IDs for batch {CurrentBatch} of {TotalBatches}", currentBatch, chunks.Count);
                await FetchImdbIdsForBatchAsync(batch, connection);
            }

            logger.LogInformation("Storing batch {CurrentBatch} of {TotalBatches}", currentBatch, chunks.Count);
            await dbContext.BulkInsertOrUpdateAsync(batch, bulkConfig, WriteProgress);
        }
    }

    public async Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query) =>
        await ExecuteCommandAsync(async connection =>
        {
            var sql =
                """
                SELECT
                    *
                FROM "Torrents"
                WHERE "ParsedTitle" % @query
                LIMIT 100;
                """;

            var parameters = new DynamicParameters();

            parameters.Add("@query", query);

            var result = await connection.QueryAsync<TorrentInfo>(sql, parameters);

            return result.ToArray();
        }, "Error finding unfiltered dmm entries.");

    public async Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter) =>
        await ExecuteCommandAsync(async connection =>
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
                       @SimilarityThreshold
                   );
                """;

            var parameters = new DynamicParameters();

            parameters.Add("@Query", filter.Query);
            parameters.Add("@Season", filter.Season);
            parameters.Add("@Episode", filter.Episode);
            parameters.Add("@Year", filter.Year);
            parameters.Add("@Language", filter.Language);
            parameters.Add("@Resolution", filter.Resolution);
            parameters.Add("@ImdbId", filter.ImdbId);
            parameters.Add("@Limit", Configuration.Dmm.MaxFilteredResults);
            parameters.Add("@SimilarityThreshold", (float)Configuration.Dmm.MinimumScoreMatch);

            var results = await connection.QueryAsync<TorrentInfoResult>(sql, parameters);

            // assign imdb to torrent info
            return results.Select(MapImdbDataToTorrentInfo).ToArray();
        }, "Error finding unfiltered dmm entries.");

    private static Func<TorrentInfoResult, TorrentInfoResult> MapImdbDataToTorrentInfo =>
        torrentInfo =>
        {
            if (torrentInfo.ImdbId != null)
            {
                torrentInfo.Imdb = new ImdbFile
                {
                    ImdbId = torrentInfo.ImdbId,
                    Category = torrentInfo.ImdbCategory,
                    Title = torrentInfo.ImdbTitle,
                    Year = torrentInfo.ImdbYear ?? 0,
                    Adult = torrentInfo.ImdbAdult
                };
            }

            return torrentInfo;
        };

    private async Task FetchImdbIdsForBatchAsync(IEnumerable<TorrentInfo> batch, NpgsqlConnection connection)
    {
        foreach (var torrent in batch)
        {
            if (_imdbCache.TryGetValue(torrent.CacheKey(), out var imdbId))
            {
                torrent.ImdbId = imdbId;
                continue;
            }

            torrent.ImdbId = await FetchImdbIdAsync(connection, torrent);

            if (torrent.ImdbId is null)
            {
                logger.LogWarning("No matching IMDb record found for title: {Title}, category: {Category}, year: {Year}", torrent.ParsedTitle, torrent.Category, torrent.Year);
                continue;
            }

            logger.LogInformation("Found IMDb Id: {ImdbId} for title: {Title}, category: {Category}, year: {Year}", torrent.ImdbId, torrent.ParsedTitle, torrent.Category, torrent.Year);
        }
    }

    private async Task<string?> FetchImdbIdAsync(NpgsqlConnection connection, TorrentInfo torrent)
    {
        var sqlQuery =
            $"""
              SELECT
                imdb_id as "ImdbId",
                title as "Title",
                year as "Year",
                score as "Score",
                category as "Category"
               FROM search_imdb_meta(
                   @Title,
                   @Category,
                   @Year,
                   1,
                   {Configuration.Imdb.MinimumScoreMatch}
               );
            """;

        var parameters = new DynamicParameters();

        parameters.Add("@Title", torrent.ParsedTitle);
        parameters.Add("@Category", torrent.Category);
        parameters.Add("@Year", torrent.Year);

        if (connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync();
        }

        var imdbRecord = await connection.QueryFirstOrDefaultAsync<ImdbSearchResult>(sqlQuery, parameters);

        _imdbCache[torrent.CacheKey()] = imdbRecord?.ImdbId ?? null;

        return imdbRecord?.ImdbId;
    }

    private void WriteProgress(decimal @decimal) => logger.LogInformation("Storing torrent info: {Percentage:P}", @decimal);
}
