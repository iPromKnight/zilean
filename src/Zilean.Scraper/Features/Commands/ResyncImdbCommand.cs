namespace Zilean.Scraper.Features.Commands;

public class ResyncImdbCommand(
    ImdbMetadataLoader imdbLoader,
    ITorrentInfoService torrentInfoService,
    IImdbFileService imdbFileService,
    ZileanDbContext dbContext,
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

        [CommandOption("-a|--retag-all-imdbs")]
        [Description("Will attempt to match IMDB ids for all torrents.")]
        [DefaultValue(false)]
        public bool RetagAllImdbs { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ResyncImdbCommandSettings settings)
    {
        if (settings is {RetagAllImdbs: true, RetagMissingImdbs: true})
        {
            logger.LogError("Cannot use both --retag-missing-imdbs and --retag-all-imdbs at the same time");
            return 1;
        }

        var result = await imdbLoader.Execute(CancellationToken.None, skipLastImport: settings.SkipLastImport);

        if (result != 0)
        {
            return result;
        }

        try
        {
            if (settings.RetagMissingImdbs)
            {
                await HandleRetagging(all: false);
                return 0;
            }

            if (settings.RetagAllImdbs)
            {
                await HandleRetagging(all: true);
                return 0;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred during ResyncImdbCommand");
            result = 1;
        }

        return result;
    }

    private async Task HandleRetagging(bool all = false)
    {
        var torrents = dbContext.Torrents.AsNoTracking()
            .Where(x => x.Category != "xxx");

        if (!all)
        {
            torrents = torrents.Where(x => x.ImdbId == null);
        }

        var processableTorrents = await torrents.ToListAsync();
        logger.LogInformation("Found {TorrentCount} torrents", processableTorrents.Count);

        if (processableTorrents.Count > 0)
        {
            logger.LogInformation("Starting to process torrents...");

            var imdbTvFiles = await imdbFileService.GetImdbTvFiles();
            var imdbMovieFiles = await imdbFileService.GetImdbMovieFiles();

            var updatedTorrents = await torrentInfoService.MatchImdbIdsForBatchAsync(processableTorrents, imdbTvFiles, imdbMovieFiles);

            logger.LogInformation("Updating {TorrentCount} torrents", updatedTorrents.Count);

            await using var scope = serviceProvider.CreateAsyncScope();
            var scopedDbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            scopedDbContext.AttachRange(updatedTorrents);
            scopedDbContext.UpdateRange(updatedTorrents);
            await scopedDbContext.SaveChangesAsync();

            logger.LogInformation("Finished processing torrents");
        }
        else
        {
            logger.LogInformation("No torrents found to match");
        }

        await torrentInfoService.VaccumTorrentsIndexes(CancellationToken.None);
    }
}
