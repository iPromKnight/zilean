using Zilean.Shared.Extensions;

namespace Zilean.Database.Services;

public class ImdbFileService(ILogger<ImdbFileService> logger, ZileanConfiguration configuration, IServiceProvider serviceProvider) : BaseDapperService(logger, configuration), IImdbFileService
{
    private ConcurrentBag<ImdbFile> ImdbFiles { get; } = [];
    public void AddImdbFile(ImdbFile imdbFile) => ImdbFiles.Add(imdbFile);
    public async Task StoreImdbFiles()
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        if (ImdbFiles.IsEmpty)
        {
            logger.LogInformation("No imdb files to store.");
            return;
        }

        var bulkConfig = new BulkConfig
        {
            SetOutputIdentity = false,
            BatchSize = 5000,
            PropertiesToIncludeOnUpdate = [string.Empty],
            UpdateByProperties = ["ImdbId"],
            BulkCopyTimeout = 0,
            TrackingEntities = false,
        };

        dbContext.Database.SetCommandTimeout(0);

        logger.LogInformation("Storing {Count} imdb entries", ImdbFiles.Count);

        await dbContext.BulkInsertOrUpdateAsync(ImdbFiles, bulkConfig);

        var imdbLastImport = new ImdbLastImport
        {
            OccuredAt = DateTime.UtcNow,
            EntryCount = ImdbFiles.Count,
            Status = ImportStatus.Complete
        };

        await SetImdbLastImportAsync(imdbLastImport);
    }

    public async Task VaccumImdbFilesIndexes(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync("VACUUM (VERBOSE, ANALYZE) \"ImdbFiles\"", cancellationToken: cancellationToken);
    }

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

    public async Task<ImdbLastImport?> GetImdbLastImportAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var imdbLastImport = await dbContext.ImportMetadata.AsNoTracking().FirstOrDefaultAsync(x => x.Key == MetadataKeys.ImdbLastImport, cancellationToken: cancellationToken);

        return imdbLastImport?.Value.Deserialize<ImdbLastImport>();
    }

    public async Task<ConcurrentDictionary<int, List<ImdbFile>>> GetImdbMovieFiles()
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

    public async Task<ConcurrentDictionary<int, List<ImdbFile>>> GetImdbTvFiles()
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

    public async Task SetImdbLastImportAsync(ImdbLastImport imdbLastImport)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var metadata = await dbContext.ImportMetadata.FirstOrDefaultAsync(x => x.Key == MetadataKeys.ImdbLastImport);

        if (metadata is null)
        {
            metadata = new ImportMetadata
            {
                Key = MetadataKeys.ImdbLastImport,
                Value = JsonSerializer.SerializeToDocument(imdbLastImport),
            };
            await dbContext.ImportMetadata.AddAsync(metadata);
            await dbContext.SaveChangesAsync();
            return;
        }

        metadata.Value = JsonSerializer.SerializeToDocument(imdbLastImport);
        await dbContext.SaveChangesAsync();
    }

    public int ImdbFileCount => ImdbFiles.Count;
}
