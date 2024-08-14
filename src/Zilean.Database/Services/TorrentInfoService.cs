namespace Zilean.Database.Services;

public class TorrentInfoService(ILogger<TorrentInfoService> logger, ZileanConfiguration configuration)
    : BaseDapperService(logger, configuration), ITorrentInfoService
{
    public Task StoreTorrentInfo(IEnumerable<TorrentInfo> torrents) =>
        ExecuteCommandAsync(
            async connection =>
            {
                await connection.ExecuteAsync("CREATE TEMP TABLE temp_torrents AS TABLE public.\"Torrents\" WITH NO DATA;");

                const string copySql =
                    """
                        COPY temp_torrents ("InfoHash", "Codec", "Episodes", "Category", "Languages", "RawTitle", "Remastered", "Resolution", "Seasons", "Size", "Title", "Year")
                        FROM STDIN (FORMAT BINARY);
                    """;

                await using (var writer = await connection.BeginBinaryImportAsync(copySql))
                {
                    foreach (var entry in torrents)
                    {
                        await writer.StartRowAsync();
                        await writer.WriteAsync(entry.InfoHash, NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Codec, NpgsqlDbType.Array | NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Episodes, NpgsqlDbType.Array | NpgsqlDbType.Integer);
                        await writer.WriteAsync(entry.Category, NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Languages, NpgsqlDbType.Array | NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.RawTitle, NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Remastered, NpgsqlDbType.Boolean);
                        await writer.WriteAsync(entry.Resolution, NpgsqlDbType.Array | NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Seasons, NpgsqlDbType.Array | NpgsqlDbType.Integer);
                        await writer.WriteAsync(entry.Size, NpgsqlDbType.Bigint);
                        await writer.WriteAsync(entry.Title, NpgsqlDbType.Text);
                        await writer.WriteAsync(entry.Year, NpgsqlDbType.Integer);
                    }

                    await writer.CompleteAsync();
                }

                const string insertFromTempSql =
                    """
                     INSERT INTO public."Torrents" ("InfoHash", "Codec", "Episodes", "Category", "Languages", "RawTitle", "Remastered", "Resolution", "Seasons", "Size", "Title", "Year")
                     SELECT "InfoHash", "Codec", "Episodes", "Category", "Languages", "RawTitle", "Remastered", "Resolution", "Seasons", "Size", "Title", "Year"
                     FROM temp_torrents
                     ON CONFLICT ("InfoHash") DO NOTHING;
                    """;

                await connection.ExecuteAsync(insertFromTempSql, commandTimeout: 0);
                await connection.ExecuteAsync("DROP TABLE IF EXISTS temp_torrents;");

                logger.LogInformation("All torrents stored.");

            },
            "Storing torrents in the database...");

    public async Task<ExtractedDmmEntryResponse[]> SearchForTorrentInfoByOnlyTitle(string query) =>
        await ExecuteCommandAsync(async connection =>
        {
            const string sql =
                """
                SELECT
                    imdb_id as "ImdbId",
                    title as "Title",
                    year as "Year",
                    score as "Score",
                    category as "Category"
                FROM search_imdb_meta(@query, @category, @year, 10)
                """;

            var parameters = new DynamicParameters();

            parameters.Add("@query", query);

            var result = await connection.QueryAsync<TorrentInfo>(sql, parameters);

            return result.Select(x => new ExtractedDmmEntryResponse(x)).ToArray();
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

            return results.Select(result => new TorrentInfo
            {
                InfoHash = result.InfoHash,
                Resolution = result.Resolution,
                Year = result.Year,
                Remastered = result.Remastered,
                Codec = result.Codec,
                Audio = result.Audio,
                Quality = result.Quality,
                Episodes = result.Episodes,
                Seasons = result.Seasons,
                Languages = result.Languages,
                Title = result.Title,
                RawTitle = result.RawTitle,
                Size = result.Size,
                Category = result.Category,
                ImdbId = result.ImdbId,
                Imdb = result.ImdbId != null
                    ? new ImdbFile
                    {
                        ImdbId = result.ImdbId,
                        Category = result.ImdbCategory,
                        Title = result.ImdbTitle,
                        Year = result.ImdbYear ?? 0,
                        Adult = result.ImdbAdult
                    }
                    : null
            }).ToArray();
        }, "Error finding unfiltered dmm entries.");
}
