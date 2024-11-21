namespace Zilean.Scraper.Features.Ingestion.Dmm;

public class DmmScraping(
    DmmFileDownloader downloader,
    ParseTorrentNameService parseTorrentNameService,
    ITorrentInfoService torrentInfoService,
    ZileanConfiguration configuration,
    ILogger<DmmScraping> logger,
    ILoggerFactory loggerFactory,
    DmmService dmmService)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        try
        {
            var processor = new DmmFileEntryProcessor(dmmService, torrentInfoService, parseTorrentNameService, loggerFactory, configuration);

            var (dmmLastImport, created) = await RetrieveAndInitializeDmmLastImport(cancellationToken);

            var tempDirectory = await DownloadDmmFileToTempPath(dmmLastImport, created, cancellationToken);

            await UpdateDmmLastImportStatus(dmmLastImport, ImportStatus.InProgress, 0, 0);

            await processor.LoadParsedPages(cancellationToken);

            var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
                .Where(f => !processor.ExistingPages.ContainsKey(Path.GetFileName(f)))
                .ToList();

            logger.LogInformation("Found {Count} files to parse", files.Count);

            if (files.Count == 0)
            {
                logger.LogInformation("No files to parse, exiting");
                return 0;
            }

            await processor.ProcessFilesAsync(files, cancellationToken);

            logger.LogInformation("All files processed");

            await UpdateDmmLastImportStatus(dmmLastImport, ImportStatus.Complete, processor.NewPages.Count, processor.NewPages.Sum(x => x.Value));

            await torrentInfoService.VaccumTorrentsIndexes(cancellationToken);

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

    private async Task<string> DownloadDmmFileToTempPath(DmmLastImport dmmLastImport, bool created, CancellationToken cancellationToken) =>
        created
            ? await downloader.DownloadFileToTempPath(null, cancellationToken)
            : await downloader.DownloadFileToTempPath(dmmLastImport, cancellationToken);

    private async Task<(DmmLastImport DmmLastImport, bool Created)> RetrieveAndInitializeDmmLastImport(CancellationToken cancellationToken)
    {
        var dmmLastImport = await dmmService.GetDmmLastImportAsync(cancellationToken);

        if (dmmLastImport is not null)
        {
            return (dmmLastImport, false);
        }

        dmmLastImport = new DmmLastImport();
        return (dmmLastImport, true);
    }

    private async Task UpdateDmmLastImportStatus(DmmLastImport? dmmLastImport, ImportStatus status, int pageCount, int entryCount)
    {
        dmmLastImport.OccuredAt = DateTime.UtcNow;
        dmmLastImport.PageCount = pageCount;
        dmmLastImport.EntryCount = entryCount;
        dmmLastImport.Status = status;

        await dmmService.SetDmmImportAsync(dmmLastImport);
    }
}
