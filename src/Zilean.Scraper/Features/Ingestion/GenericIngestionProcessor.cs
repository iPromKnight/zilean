namespace Zilean.Scraper.Features.Ingestion;

public class GenericIngestionProcessor(
    IHttpClientFactory clientFactory,
    ILogger<GenericIngestionProcessor> logger,
    ParseTorrentNameService parseTorrentNameService,
    ITorrentInfoService torrentInfoService,
    ZileanConfiguration configuration)
{
    public async Task ProcessTorrentsAsync(string url, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing URL: {Url}", url);

        var channel = Channel.CreateBounded<Task<StreamedEntry>>(new BoundedChannelOptions(configuration.Ingestion.MaxChannelSize)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producerTask = ProduceAsync(url, channel.Writer, cancellationToken);
        var consumerTask = ConsumeAsync(channel.Reader, configuration.Ingestion.BatchSize, cancellationToken);
        await Task.WhenAll(producerTask, consumerTask);
    }

    private async Task ProduceAsync(string url, ChannelWriter<Task<StreamedEntry>> writer, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = clientFactory.CreateClient();
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
        catch (Exception)
        {
            logger.LogWarning("Error processing item");
            throw;
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
            }

            if (torrents.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            logger.LogInformation("Processing batch of {Count} torrents", torrents.Count);

            if (torrents.Count != 0)
            {
                var parsedTorrents = await parseTorrentNameService.ParseAndPopulateAsync(torrents);
                var finalizedTorrents = parsedTorrents.Where(torrentInfo => torrentInfo.WipeSomeTissue()).ToList();
                logger.LogInformation("Parsed {Count} torrents", finalizedTorrents.Count);
                await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
            }
        }
        catch (Exception)
        {
            logger.LogWarning("Error processing batch of torrents. Batch size: {BatchSize}", batch.Count);
            throw;
        }
    }
}
