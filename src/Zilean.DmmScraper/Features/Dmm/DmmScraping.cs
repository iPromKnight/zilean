namespace Zilean.DmmScraper.Features.Dmm;

public class DmmScraping(
    DmmSyncState dmmState,
    DmmFileDownloader downloader,
    ParseTorrentNameService parseTorrentNameService,
    ITorrentInfoService torrentInfoService,
    ZileanConfiguration configuration,
    DmmPageProcessor processor,
    ILogger<DmmScraping> logger,
    DmmService dmmService)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        try
        {
            var dmmLastImport = await dmmService.GetDmmLastImportAsync(cancellationToken);

            await dmmState.SetRunning(cancellationToken);

            var tempDirectory = await downloader.DownloadFileToTempPath(dmmLastImport, cancellationToken);

            var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
                .Where(f => !dmmState.ExistingPages.ContainsKey(Path.GetFileName(f)))
                .ToList();

            logger.LogInformation("Found {Count} files to parse", files.Count);

            if (files.Count == 0)
            {
                logger.LogInformation("No files to parse, exiting");
                return 0;
            }

            if (configuration.Dmm.ImportBatched)
            {
                await ProcessBatched(files, cancellationToken);
            }
            else
            {
                await ProcessUnBatched(files, cancellationToken);
            }

            logger.LogInformation("All files processed");

            dmmLastImport ??= new DmmLastImport();

            dmmLastImport.OccuredAt = DateTime.UtcNow;
            dmmLastImport.PageCount = dmmState.ParsedPages.Count;
            dmmLastImport.EntryCount = dmmState.ParsedPages.Sum(x => x.Value);
            dmmLastImport.Status = ImportStatus.Complete;

            await dmmService.SetDmmImportAsync(dmmLastImport);

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

    private async Task ProcessBatched(List<string> files, CancellationToken cancellationToken) =>
        await AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                AnsiConsole.MarkupLine(
                    "[yellow]Batched processing enabled - This is for low end systems, and is expected to take a long time to run on the first run. A very long time indeed![/]");

                var task = ctx.AddTask("[green]Processing DMM Hashlists[/]");
                var progress = new Progress<double>(value => task.Increment(value));

                foreach (var file in files.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
                {
                    logger.LogInformation("Processing file {File}", file);

                    var torrents = new List<ExtractedDmmEntry>();

                    await foreach (var torrent in ProcessFileAsync(file, processor, dmmState, logger, progress, files.Count,
                                       cancellationToken))
                    {
                        torrents.Add(torrent);
                    }

                    if (torrents.Count == 0)
                    {
                        continue;
                    }

                    var distinctTorrents = torrents.DistinctBy(x => x.InfoHash).ToList();

                    logger.LogInformation("Distinct torrents: {Count}", distinctTorrents.Count);

                    var parsedTorrents = await parseTorrentNameService.ParseAndPopulateAsync(distinctTorrents);

                    var finalizedTorrents = parsedTorrents.Where(WipeSomeTissue).ToList();

                    await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
                }
            });

    private async Task ProcessUnBatched(List<string> files, CancellationToken cancellationToken)
    {
        var torrents = new List<ExtractedDmmEntry>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        await AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Processing DMM Hashlists[/]");
                var progress = new Progress<double>(value => task.Increment(value));

                await Parallel.ForEachAsync(files, parallelOptions, async (file, ct) =>
                {
                    await foreach (var torrent in ProcessFileAsync(file, processor, dmmState, logger, progress, files.Count, ct))
                    {
                        lock (torrents)
                        {
                            torrents.Add(torrent);
                        }
                    }
                });
            });

        logger.LogInformation("All files processed");

        if (torrents.Count != 0)
        {
            var distinctTorrents = torrents.DistinctBy(x => x.InfoHash).ToList();

            var parsedTorrents = await parseTorrentNameService.ParseAndPopulateAsync(distinctTorrents);

            var finalizedTorrents = parsedTorrents.Where(WipeSomeTissue).ToList();

            logger.LogInformation("Parsed {Count} torrents", finalizedTorrents.Count);

            await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
        }
    }

    private static async IAsyncEnumerable<ExtractedDmmEntry> ProcessFileAsync(string file,
        DmmPageProcessor processor,
        DmmSyncState dmmState,
        ILogger<DmmScraping> logger,
        IProgress<double> progress,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Cancellation requested, stopping processing");
            yield break;
        }

        var fileName = Path.GetFileName(file);
        var sanitizedTorrents = await processor.ProcessPageAsync(file, fileName, cancellationToken);
        dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
        dmmState.IncrementProcessedFilesCount();
        progress.Report(100.0 / count);

        logger.LogInformation("Processed {FileName} with {Count} torrents", fileName, sanitizedTorrents.Count);

        foreach (var torrent in sanitizedTorrents)
        {
            yield return torrent;
        }
    }

    private static bool WipeSomeTissue(TorrentInfo torrent) =>
        !torrent.RawTitle.Contains(" XXX ", StringComparison.OrdinalIgnoreCase) ||
        torrent.ParsedTitle.Contains("XXX", StringComparison.OrdinalIgnoreCase);
}
