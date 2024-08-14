namespace Zilean.DmmScraper.Features.Dmm;

public class DmmScraping(
    DmmSyncState dmmState,
    DmmFileDownloader downloader,
    ParseTorrentNameService parseTorrentNameService,
    ITorrentInfoService torrentInfoService,
    DmmPageProcessor processor,
    ILogger<DmmScraping> logger)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        try
        {
            await dmmState.SetRunning(cancellationToken);

            var tempDirectory = await downloader.DownloadFileToTempPath(cancellationToken);

            var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
                .Where(f => !dmmState.ParsedPages.ContainsKey(Path.GetFileName(f)))
                .ToList();

            logger.LogInformation("Found {Count} files to parse", files.Count);

            await AnsiConsole.Progress()
                .AutoClear(true)
                .HideCompleted(true)
                .Columns([
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn()
                ])
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[green]Processing DMM Hashlists[/]");
                    var progress = new Progress<double>(value => task.Increment(value));

                    foreach (var file in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        logger.LogInformation("Processing file {File}", file);

                        var torrents = await ProcessFileAsync(file, processor, dmmState, logger, progress, files.Count, cancellationToken);

                        if (torrents.Count != 0)
                        {
                            var distinctTorrents = torrents.DistinctBy(x => x.InfoHash).ToList();

                            logger.LogInformation("Distinct torrents: {Count}", distinctTorrents.Count);

                            var finalizedTorrents = await parseTorrentNameService.ParseAndPopulateAsync(distinctTorrents);

                            await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
                        }
                    }
                });

            logger.LogInformation("All files processed");

            await dmmState.SetFinished(cancellationToken, processor);

            logger.LogInformation("DMM Internal Tasks Completed");

            return 0;
        }
        catch (TaskCanceledException)
        {
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during DMM Scraper Task");
            return 1;
        }
    }

    private static async Task<List<ExtractedDmmEntry>> ProcessFileAsync(string file,
        DmmPageProcessor processor,
        DmmSyncState dmmState,
        ILogger<DmmScraping> logger,
        IProgress<double> progress,
        int count,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Cancellation requested, stopping processing");
            return [];
        }

        var fileName = Path.GetFileName(file);
        var sanitizedTorrents = await processor.ProcessPageAsync(file, fileName, cancellationToken);
        dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
        dmmState.IncrementProcessedFilesCount();
        progress.Report(100.0 / count);

        logger.LogInformation("Processed {FileName} with {Count} torrents", fileName, sanitizedTorrents.Count);

        return sanitizedTorrents;
    }
}
