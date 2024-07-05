namespace Zilean.ApiService.Features.Dmm;

public class DmmSyncJob(
    ILogger<DmmSyncJob> logger,
    IDmmFileDownloader dmmFileDownloader,
    IElasticClient elasticClient,
    DmmSyncState dmmState) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    public async Task Invoke()
    {
        if (!dmmState.IsRunning)
        {
            await dmmState.SetRunning(CancellationToken);
        }

        var tempDirectory = await dmmFileDownloader.DownloadFileToTempPath(CancellationToken);

        var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
            .Where(f => !dmmState.ParsedPages.ContainsKey(Path.GetFileName(f)))
            .ToArray();

        logger.LogInformation("Found {Files} files to parse", files.Length);

        var processor = new DmmPageProcessor(dmmState, logger, CancellationToken);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            var sanitizedTorrents = await processor.ProcessPageAsync(file, fileName);

            if (sanitizedTorrents.Count != 0)
            {
                var distinctTorrents = sanitizedTorrents.DistinctBy(x => x.InfoHash).ToList();

                logger.LogInformation("Total torrents from file {FileName}: {Torrents}", fileName, sanitizedTorrents.Count);
                logger.LogInformation("Indexing {Torrents} distinct new torrents from file {FileName}", distinctTorrents.Count, fileName);

                var indexResult = await elasticClient.IndexManyBatchedAsync(distinctTorrents, ElasticClient.DmmIndex);

                if (indexResult.Errors)
                {
                    logger.LogError("Failed to index {Torrents} torrents from file {FileName}", distinctTorrents.Count, fileName);
                    break;
                }

                logger.LogInformation("Indexed {Torrents} torrents from file {FileName}", distinctTorrents.Count, fileName);
            }

            dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
            dmmState.IncrementProcessedFilesCount();
        }

        await dmmState.SetFinished(CancellationToken, processor);
    }
}
