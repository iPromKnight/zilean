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
            var elasticClient = new ElasticClient(configuration, loggerFactory.CreateLogger<ElasticClient>());

            await dmmState.SetRunning(cancellationToken);

            var tempDirectory = await dmmFileDownloader.DownloadFileToTempPath(cancellationToken);

            var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
                .Where(f => !dmmState.ParsedPages.ContainsKey(Path.GetFileName(f)))
                .ToArray();

            logger.LogInformation("Found {Count} files to parse", files.Length);

            var processor = new DmmPageProcessor(dmmState, loggerFactory.CreateLogger<DmmPageProcessor>(), cancellationToken);

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Cancellation requested, stopping processing");
                    break;
                }

                var fileName = Path.GetFileName(file);

                var sanitizedTorrents = await processor.ProcessPageAsync(file, fileName);

                if (sanitizedTorrents.Count != 0)
                {
                    var distinctTorrents = sanitizedTorrents.DistinctBy(x => x.InfoHash).ToList();

                    logger.LogInformation("Total torrents from file {FileName}: {Count}", fileName, sanitizedTorrents.Count);
                    logger.LogInformation("Indexing {Count} distinct new torrents from file {Filename}", distinctTorrents.Count,
                        fileName);

                    var indexResult =
                        await elasticClient.IndexManyBatchedAsync(distinctTorrents, ElasticClient.DmmIndex, cancellationToken);

                    if (indexResult.Errors)
                    {
                        logger.LogInformation("Failed to index {Count} torrents from file {FileName}", distinctTorrents.Count,
                            fileName);
                        break;
                    }

                    logger.LogInformation("Indexed {Count} torrents from file {FileName}", distinctTorrents.Count, fileName);
                }

                dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
                dmmState.IncrementProcessedFilesCount();
            }

            await dmmState.SetFinished(cancellationToken, processor);

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
}
