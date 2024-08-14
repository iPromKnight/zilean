using NpgsqlTypes;

namespace Zilean.Database.Services;

public class ImdbFileService(ILogger<ImdbFileService> logger, ZileanConfiguration configuration) : BaseDapperService(logger, configuration), IImdbFileService
{
    private ConcurrentBag<ImdbFile> ImdbFiles { get; } = [];

    public void AddImdbFile(ImdbFile imdbFile) => ImdbFiles.Add(imdbFile);
    public Task StoreImdbFiles() =>
        ExecuteCommandAsync(
            async connection =>
            {
                await connection.ExecuteAsync("""CREATE TEMP TABLE temp_imdbfiles AS TABLE "ImdbFiles" WITH NO DATA;""");

                const string sql =
                    """
                    COPY temp_imdbfiles ("ImdbId", "Category", "Title", "Year", "Adult") FROM STDIN (FORMAT BINARY);
                    """;

                await using (var writer = await connection.BeginBinaryImportAsync(sql))
                {
                    foreach (var entry in ImdbFiles)
                    {
                        try
                        {
                            await writer.StartRowAsync();
                            await writer.WriteAsync(entry.ImdbId, NpgsqlDbType.Text);
                            await writer.WriteAsync(entry.Category, NpgsqlDbType.Text);
                            await writer.WriteAsync(entry.Title, NpgsqlDbType.Text);
                            await writer.WriteAsync(entry.Year, NpgsqlDbType.Integer);
                            await writer.WriteAsync(entry.Adult, NpgsqlDbType.Boolean);
                        }
                        catch (PostgresException e)
                        {
                            if (e.Message.Contains("duplicate key value violates unique constraint", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            throw;
                        }
                    }

                    await writer.CompleteAsync();
                }

                const string insertFromTempSql =
                    """
                    INSERT INTO "ImdbFiles" ("ImdbId", "Category", "Title", "Year", "Adult")
                    SELECT "ImdbId", "Category", "Title", "Year", "Adult" FROM temp_imdbfiles
                    ON CONFLICT ("ImdbId") DO NOTHING;
                    """;

                await connection.ExecuteAsync(insertFromTempSql, commandTimeout: 0);
                await connection.ExecuteAsync("DROP TABLE IF EXISTS temp_imdbfiles;");

                logger.LogInformation("All entries stored.");
            }, "Storing imdb metadata entries into database.");

    public async Task<ImdbSearchResult[]> SearchForImdbIdAsync(string query, int? year = null, string? category = null) =>
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
            parameters.Add("@category", category);
            parameters.Add("@year", year);

            var result = await connection.QueryAsync<ImdbSearchResult>(sql, parameters);

            return result.ToArray();
        }, "Error finding imdb metadata.");


}
