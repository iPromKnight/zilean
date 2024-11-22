namespace Zilean.Scraper.Features.Commands;

public class ResyncImdbCommand(
    ImdbMetadataLoader imdbLoader,
    ITorrentInfoService torrentInfoService,
    ZileanDbContext dbContext,
    ZileanConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<ResyncImdbCommand> logger) : AsyncCommand<ResyncImdbCommand.ResyncImdbCommandSettings>
{
    public sealed class ResyncImdbCommandSettings : CommandSettings
    {
        [CommandOption("-s|--skip-last-import")]
        [Description("Skip the date check on imdb imports (last 14 days) and force it to import.")]
        [DefaultValue(false)]
        public bool SkipLastImport { get; set; }

        [CommandOption("-t|--retag-missing-imdbs")]
        [Description("Will attempt to match IMDB ids for anything that is missing them in the database.")]
        [DefaultValue(false)]
        public bool RetagMissingImdbs { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ResyncImdbCommandSettings settings)
    {
        var result = await imdbLoader.Execute(CancellationToken.None, skipLastImport: settings.SkipLastImport);

        if (result != 0)
        {
            return result;
        }

        try
        {
            if (settings.RetagMissingImdbs)
            {
                var torrentCount = await dbContext.Torrents.CountAsync(x => x.ImdbId == null);
                logger.LogInformation("Found {TorrentCount} torrents missing IMDB Ids", torrentCount);

                if (torrentCount > 0)
                {
                    logger.LogInformation("Starting to process torrents missing IMDB Ids...");

                    var torrentsWithoutImdbBatches = dbContext.Torrents
                        .AsNoTracking()
                        .Where(x => x.ImdbId == null && x.Category != "xxx")
                        .AsAsyncEnumerable()
                        .ToChunksAsync(10000);

                    int batchNumber = 0;

                    await foreach (var torrents in torrentsWithoutImdbBatches)
                    {
                        Interlocked.Increment(ref batchNumber);

                        try
                        {
                            logger.LogInformation("Processing batch {BatchNumber} with {BatchCount} torrents", batchNumber, torrents.Count);
                            await using var sqlConnection = new NpgsqlConnection(configuration.Database.ConnectionString);
                            await sqlConnection.OpenAsync();
                            await using var scope = serviceProvider.CreateAsyncScope();
                            var scopedDbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

                            await torrentInfoService.FetchImdbIdsForBatchAsync(torrents, sqlConnection);

                            scopedDbContext.AttachRange(torrents);
                            scopedDbContext.UpdateRange(torrents);
                            await scopedDbContext.SaveChangesAsync();

                            logger.LogInformation("Successfully processed batch {BatchNumber}.", batchNumber);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to process batch {BatchNumber}.", batchNumber);
                        }
                    }

                    logger.LogInformation("Finished processing torrents missing IMDB Ids.");
                }
                else
                {
                    logger.LogInformation("No torrents found missing IMDB Ids.");
                }
            }

            await torrentInfoService.VaccumTorrentsIndexes(CancellationToken.None);

            result = 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred during ResyncImdbCommand");
            result = 1;
        }

        return result;
    }
}
