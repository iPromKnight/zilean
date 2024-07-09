using Zilean.DmmScraper.Features.PythonSupport;

namespace Zilean.DmmScraper.Features.Dmm;

public class DmmScraperTask
{
    public static async Task<int> Execute(ZileanConfiguration configuration, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<DmmScraperTask>();

        try
        {
            var httpClient = CreateHttpClient();

            var dmmState = new DmmSyncState(loggerFactory.CreateLogger<DmmSyncState>());
            var dmmFileDownloader = new DmmFileDownloader(httpClient, loggerFactory.CreateLogger<DmmFileDownloader>());
            var elasticClient = new ElasticSearchClient(configuration, loggerFactory.CreateLogger<ElasticSearchClient>());
            var rtnService = new ParseTorrentNameService(loggerFactory.CreateLogger<ParseTorrentNameService>());

            await dmmState.SetRunning(cancellationToken);

            var tempDirectory = await dmmFileDownloader.DownloadFileToTempPath(cancellationToken);

            var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
                .Where(f => !dmmState.ParsedPages.ContainsKey(Path.GetFileName(f)))
                .ToList();

            logger.LogInformation("Found {Count} files to parse", files.Count);

            var processor = new DmmPageProcessor(dmmState, loggerFactory.CreateLogger<DmmPageProcessor>(), cancellationToken);

            var torrents = new ConcurrentBag<ExtractedDmmEntry>();

            var batchedFiles = files.ToChunks(200);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(batchedFiles, parallelOptions, async (batch, ct) =>
            {
                await ProcessFileBatchAsync(batch, processor, torrents, dmmState, logger, ct);
            });

            logger.LogInformation("All files processed");

            if (torrents.Count != 0)
            {
                var distinctTorrents = torrents.DistinctBy(x => x.InfoHash).ToList();

                var indexableTorrentInformation = await rtnService.ParseAndPopulateAsync(distinctTorrents);

                logger.LogInformation("Parsed {Count} torrents", indexableTorrentInformation.Count);

                logger.LogInformation("Indexing {Count} torrents in ElasticSeach", distinctTorrents.Count);

                var indexResult =
                    await elasticClient.IndexManyBatchedAsync(indexableTorrentInformation, ElasticSearchClient.DmmIndex, cancellationToken);

                if (indexResult.Errors)
                {
                    logger.LogInformation("Failed to index {Count} torrents", distinctTorrents.Count);
                    return 1;
                }

                logger.LogInformation("Indexed {Count} torrents", distinctTorrents.Count);
            }

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

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://github.com/debridmediamanager/hashlists/zipball/main/"),
            Timeout = TimeSpan.FromMinutes(10),
        };

        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("curl/7.54");
        return httpClient;
    }

    private static async Task ProcessFileBatchAsync(
        List<string> batch,
        DmmPageProcessor processor,
        ConcurrentBag<ExtractedDmmEntry> torrents,
        DmmSyncState dmmState,
        ILogger<DmmScraperTask> logger,
        CancellationToken cancellationToken)
    {
        var tasks = batch.Select(file => ProcessFileAsync(file, processor, torrents, dmmState, logger, cancellationToken)).ToList();
        await Task.WhenAll(tasks);
    }

    private static async Task ProcessFileAsync(
        string file,
        DmmPageProcessor processor,
        ConcurrentBag<ExtractedDmmEntry> torrents,
        DmmSyncState dmmState,
        ILogger<DmmScraperTask> logger,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Cancellation requested, stopping processing");
            return;
        }

        var fileName = Path.GetFileName(file);
        var sanitizedTorrents = await processor.ProcessPageAsync(file, fileName);

        foreach (var sanitizedTorrent in sanitizedTorrents)
        {
            torrents.Add(sanitizedTorrent);
        }

        logger.LogInformation("Total torrents from file {FileName}: {Count}", fileName, sanitizedTorrents.Count);

        dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
        dmmState.IncrementProcessedFilesCount();
    }
}
