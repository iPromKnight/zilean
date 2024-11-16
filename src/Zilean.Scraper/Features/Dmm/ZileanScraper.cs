namespace Zilean.Scraper.Features.Dmm;

public class ZileanScraper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZileanScraper> _logger;

    public ZileanScraper(IHttpClientFactory clientFactory, string endpoint, ILoggerFactory loggerFactory)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(endpoint);
        _logger = loggerFactory.CreateLogger<ZileanScraper>();
    }

    public async Task ProcessTorrentsAsync(string url, int batchSize = 1000, int maxChannelCapacity = 5000)
    {
        var channel = Channel.CreateBounded<Task<StreamedEntry>>(new BoundedChannelOptions(maxChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var producerTask = ProduceAsync(url, channel.Writer);
        var consumerTask = ConsumeAsync(channel.Reader, batchSize);
        await Task.WhenAll(producerTask, consumerTask);
    }

    private async Task ProduceAsync(string url, ChannelWriter<Task<StreamedEntry>> writer)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<StreamedEntry>(stream, options))
            {
                if (item is not null)
                {
                    await writer.WriteAsync(Task.FromResult(item));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing item: {Message}", ex.Message);
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task ConsumeAsync(ChannelReader<Task<StreamedEntry>> reader, int batchSize)
    {
        var batch = new List<Task<StreamedEntry>>(batchSize);

        await foreach (var task in reader.ReadAllAsync())
        {
            batch.Add(task);

            if (batch.Count >= batchSize)
            {
                await ProcessBatch(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await ProcessBatch(batch);
        }
    }

    private async Task ProcessBatch(IEnumerable<Task<StreamedEntry>> batch)
    {
        await foreach (var result in Task.WhenEach(batch))
        {
            try
            {
                var current = await result;
                _logger.LogInformation("Processing item: {Item}", current.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing item: {Message}", ex.Message);
            }
        }
    }
}
