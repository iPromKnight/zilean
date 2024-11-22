namespace Zilean.Scraper.Features.Ingestion.Processing;

public class StreamedEntryProcessor(
    ITorrentInfoService torrentInfoService,
    ParseTorrentNameService parseTorrentNameService,
    ILoggerFactory loggerFactory,
    IHttpClientFactory clientFactory,
    ZileanConfiguration configuration) : GenericProcessor<StreamedEntry>(loggerFactory, torrentInfoService, parseTorrentNameService, configuration)
{
    private GenericEndpoint? _currentEndpoint;

    protected override ExtractedDmmEntry TransformToTorrent(StreamedEntry input) =>
        ExtractedDmmEntry.FromStreamedEntry(input);

    public async Task ProcessEndpointAsync(GenericEndpoint endpoint, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Processing URL: {@Url}", endpoint);
        _processedCounts.Reset();
        _currentEndpoint = endpoint;
        await ProcessAsync(ProduceEntriesAsync, cancellationToken);
        _processedCounts.WriteOutput(_configuration, sw);
        sw.Stop();
    }

    private async Task ProduceEntriesAsync(ChannelWriter<Task<StreamedEntry>> writer, CancellationToken cancellationToken)
    {
        try
        {
            if (_currentEndpoint is null)
            {
                _logger.LogError("Endpoint not set before calling ProduceEntriesAsync.");
                throw new InvalidOperationException("Endpoint not set");
            }

            var httpClient = clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.Ingestion.RequestTimeout);

            var fullUrl = _currentEndpoint.EndpointType switch
            {
                GenericEndpointType.Zurg => $"{_currentEndpoint.Url}/debug/torrents",
                GenericEndpointType.Zilean => $"{_currentEndpoint.Url}/torrents/all",
                _ => throw new InvalidOperationException($"Unknown endpoint type: {_currentEndpoint.EndpointType}")
            };

            if (_currentEndpoint.EndpointType == GenericEndpointType.Zilean)
            {
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", _currentEndpoint.ApiKey);
            }

            var response = await httpClient.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<StreamedEntry>(stream, options, cancellationToken))
            {
                if (item is not null)
                {
                    await writer.WriteAsync(Task.FromResult(item), cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching and producing entries for URL: {Url}", _currentEndpoint.Url);
        }
        finally
        {
            writer.Complete();
        }
    }
}
