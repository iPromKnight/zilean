namespace Zilean.Scraper.Features.Ingestion;

public class GenericIngestionProcessor(
    IHttpClientFactory clientFactory,
    ILogger<GenericIngestionProcessor> logger,
    ParseTorrentNameService parseTorrentNameService,
    ITorrentInfoService torrentInfoService,
    ZileanConfiguration configuration)
{
    private int _processedCount;

    public async Task ProcessTorrentsAsync(GenericEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Processing URL: {@Url}", endpoint);

        Interlocked.Exchange(ref _processedCount, 0);

        var channel = Channel.CreateBounded<Task<StreamedEntry>>(new BoundedChannelOptions(configuration.Ingestion.MaxChannelSize)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producerTask = ProduceAsync(endpoint, channel.Writer, cancellationToken);
        var consumerTask = ConsumeAsync(channel.Reader, configuration.Ingestion.BatchSize, cancellationToken);
        await Task.WhenAll(producerTask, consumerTask);

        logger.LogInformation("Processed {Count} torrents for endpoint '{@Endpoint}' in {TimeTaken}s", _processedCount, endpoint, sw.Elapsed.TotalSeconds);
        sw.Stop();
    }

    private async Task ProduceAsync(GenericEndpoint endpoint, ChannelWriter<Task<StreamedEntry>> writer, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(configuration.Ingestion.RequestTimeout);
            var fullUrl = endpoint.EndpointType switch
            {
                GenericEndpointType.Zurg => $"{endpoint.Url}/debug/torrents",
                GenericEndpointType.Zilean => $"{endpoint.Url}/torrents/all",
                _ => throw new InvalidOperationException("Unknown endpoint type")
            };

            var response = await httpClient.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<StreamedEntry>(stream, options, cancellationToken))
            {
                if (item is not null)
                {
                    await writer.WriteAsync(Task.FromResult(item), cancellationToken);
                }
            }
        }
        catch (HttpRequestException)
        {
            logger.LogError("Invalid status code returned for URL: {@Url}", endpoint);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occured for URL: {@Url}", endpoint);
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task ConsumeAsync(ChannelReader<Task<StreamedEntry>> reader, int batchSize, CancellationToken cancellationToken = default)
    {
        var batch = new List<Task<StreamedEntry>>(batchSize);

        await foreach (var task in reader.ReadAllAsync(cancellationToken))
        {
            batch.Add(task);

            if (batch.Count < batchSize)
            {
                continue;
            }

            await ProcessBatch(batch, cancellationToken);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            await ProcessBatch(batch, cancellationToken);
        }
    }

    private async Task ProcessBatch(List<Task<StreamedEntry>> batch, CancellationToken cancellationToken = default)
    {
        try
        {
            var torrents = new List<ExtractedDmmEntry>();

            await foreach (var result in Task.WhenEach(batch).WithCancellation(cancellationToken))
            {
                var current = await result;
                torrents.Add(ExtractedDmmEntry.FromStreamedEntry(current));
                Interlocked.Increment(ref _processedCount);
            }

            if (torrents.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var infoHashes = torrents.Select(t => t.InfoHash!).ToList();

            var existingInfoHashes = await torrentInfoService.GetExistingInfoHashesAsync(infoHashes);

            var newTorrents = torrents.DistinctBy(x => x.InfoHash).Where(t => !existingInfoHashes.Contains(t.InfoHash)).ToList();
            logger.LogInformation("Filtered out {Count} torrents already in the database", torrents.Count - newTorrents.Count);

            if (newTorrents.Count == 0)
            {
                logger.LogInformation("No new torrents to process in this batch.");
                return;
            }

            if (torrents.Count != 0)
            {
                var parsedTorrents = await parseTorrentNameService.ParseAndPopulateAsync(newTorrents);

                var blacklistedHashes = await torrentInfoService.GetBlacklistedItems();

                var finalizedTorrents = parsedTorrents
                    .Where(torrentInfo => torrentInfo.WipeSomeTissue())
                    .Where(torrentsInfo => !torrentsInfo.IsBlacklisted(blacklistedHashes))
                    .ToList();

                logger.LogInformation("Removed {Count} hashes due to blacklisting or possible adult titile matches", parsedTorrents.Count - finalizedTorrents.Count);
                logger.LogInformation("Parsed {Count} torrents", finalizedTorrents.Count);
                await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error processing batch of torrents. Batch size: {BatchSize}", batch.Count);
        }
    }
}
