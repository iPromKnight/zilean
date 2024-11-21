namespace Zilean.Scraper.Features.Ingestion.Processing;

public abstract class GenericProcessor<TInput>(
    ILoggerFactory loggerFactory,
    ITorrentInfoService torrentInfoService,
    ParseTorrentNameService parseTorrentNameService,
    ZileanConfiguration configuration)
    where TInput : class
{
    protected readonly ILogger<GenericProcessor<TInput>> _logger = loggerFactory.CreateLogger<GenericProcessor<TInput>>();
    protected abstract ExtractedDmmEntry TransformToTorrent(TInput input);
    protected readonly ProcessedCounts _processedCounts = new();
    protected readonly ZileanConfiguration _configuration = configuration;
    private HashSet<string> _blacklistedHashes = [];
    private readonly ObjectPool<List<ExtractedDmmEntry>> _torrentsListPool = new DefaultObjectPoolProvider().Create<List<ExtractedDmmEntry>>();

    protected async Task ProcessAsync(Func<ChannelWriter<Task<TInput>>, CancellationToken, Task> producerAction, CancellationToken cancellationToken)
    {
        _blacklistedHashes = await torrentInfoService.GetBlacklistedItems();

        var channel = Channel.CreateBounded<Task<TInput>>(new BoundedChannelOptions(_configuration.Parsing.BatchSize * 2)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producerTask = producerAction(channel.Writer, cancellationToken);
        var consumerTask = ConsumeAsync(channel.Reader, cancellationToken);

        await Task.WhenAll(producerTask, consumerTask);
    }

    private async Task ConsumeAsync(ChannelReader<Task<TInput>> reader, CancellationToken cancellationToken)
    {
        var batch = new List<Task<TInput>>(_configuration.Parsing.BatchSize);

        try
        {
            await foreach (var task in reader.ReadAllAsync(cancellationToken))
            {
                batch.Add(task);

                if (batch.Count < _configuration.Parsing.BatchSize)
                {
                    continue;
                }

                await OnProcessTorrentsAsync(batch, cancellationToken);
                batch.Clear();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing cancelled, attempting to flush remaining batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during processing, attempting to flush remaining batch.");
        }
        finally
        {
            if (batch.Count > 0)
            {
                try
                {
                    _logger.LogInformation("Final flush of {Count} remaining items in batch.", batch.Count);
                    await OnProcessTorrentsAsync(batch, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during final batch flush.");
                }
            }
        }
    }


    private async Task OnProcessTorrentsAsync(List<Task<TInput>> batch, CancellationToken cancellationToken)
    {
        var torrents = _torrentsListPool.Get();

        try
        {
            await foreach (var result in Task.WhenEach(batch).WithCancellation(cancellationToken))
            {
                var current = await result;
                torrents.Add(TransformToTorrent(current));
            }

            if (torrents.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var distinctTorrents = torrents.DistinctBy(t => t.InfoHash).ToList();

            _logger.LogInformation("Removed duplicate hashes: {Count}", torrents.Count - distinctTorrents.Count);

            var infoHashes = distinctTorrents.Select(t => t.InfoHash!).ToList();
            var existingInfoHashes = await torrentInfoService.GetExistingInfoHashesAsync(infoHashes);

            var newTorrents = distinctTorrents.Where(t => !existingInfoHashes.Contains(t.InfoHash)).ToList();
            _logger.LogInformation("Filtered out {Count} torrents already in the database",
                distinctTorrents.Count - newTorrents.Count);

            if (newTorrents.Count > 0)
            {
                var parsedTorrents =
                    await parseTorrentNameService.ParseAndPopulateAsync(newTorrents, _configuration.Parsing.BatchSize);

                var finalizedTorrents = parsedTorrents
                    .AsEnumerable()
                    .FilterBlacklistedTorrents(parsedTorrents, _blacklistedHashes, _configuration, _logger, _processedCounts)
                    .ToList();

                await torrentInfoService.StoreTorrentInfo(finalizedTorrents);
                _processedCounts.AddProcessed(finalizedTorrents.Count);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing batch of torrents. Batch size: {BatchSize}", batch.Count);
        }
        finally
        {
            torrents.Clear();
            _torrentsListPool.Return(torrents);
        }
    }
}
