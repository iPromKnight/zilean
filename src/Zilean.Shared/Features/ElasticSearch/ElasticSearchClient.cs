using System.Text;

namespace Zilean.Shared.Features.ElasticSearch;

public interface IElasticSearchClient
{
    Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, CancellationToken cancellationToken, int batchSize = 5000) where T : class;
    Task<ElasticClient> GetClient();
}

public class ElasticSearchClient : IElasticSearchClient
{
    public const string DmmIndex = "dmm-entries";

    private readonly ZileanConfiguration _configuration;
    private readonly ILogger<ElasticSearchClient> _logger;
    private readonly Task _initializationTask;
    private readonly ElasticClient _client;

    public ElasticSearchClient(ZileanConfiguration configuration, ILogger<ElasticSearchClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _client = CreateNewClient(configuration);
        _initializationTask = AsyncInitialization();
    }

    public async Task<ElasticClient> GetClient()
    {
        await _initializationTask;
        return _client;
    }

    public async Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, CancellationToken cancellationToken, int batchSize = 5000) where T : class
    {
        await _initializationTask;

        BulkResponse response = null;
        for (int i = 0; i < documents.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, stopping indexing");
                break;
            }

            var batch = documents.GetRange(i, Math.Min(batchSize, documents.Count - i));
            response = await BulkIndex(batch, index, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index batch {Batch} of {Total} documents", i, documents.Count);
                _logger.LogError("Error: {Error}", response.OriginalException.Message);
                break;
            }
        }

        return response;
    }

    private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index, CancellationToken cancellationToken) where T : class => _client.IndexManyAsync(batch, index, cancellationToken: cancellationToken);

    private static ElasticClient CreateNewClient(ZileanConfiguration configuration)
    {
        var pool = new SingleNodeConnectionPool(new Uri(configuration.ElasticSearch.Url));
        var settings = new ConnectionSettings(pool)
            .DefaultMappingFor<ExtractedDmmEntry>(ExtractedDmmEntryMapping)
            .EnableApiVersioningHeader();

// #if DEBUG
//         settings.EnableDebugMode(details =>
//         {
//             Console.WriteLine($"ES Request: {Encoding.UTF8.GetString(details.RequestBodyInBytes ?? [])}");
//             Console.WriteLine($"ES Response: {Encoding.UTF8.GetString(details.ResponseBodyInBytes ?? [])}");
//         });
// #endif

        return new ElasticClient(settings);
    }

    private static IClrTypeMapping<ExtractedDmmEntry> ExtractedDmmEntryMapping(ClrTypeMappingDescriptor<ExtractedDmmEntry> x)
    {
        x.IndexName(DmmIndex);
        x.IdProperty(p => p.InfoHash);

        return x;
    }

    private async Task AsyncInitialization()
    {
        _logger.LogInformation("Checking Elasticsearch connection to server {Server}", _configuration.ElasticSearch.Url);

        var isOnline = await _client.PingAsync();

        if (!isOnline.IsValid)
        {
            _logger.LogError("Elasticsearch connection to server {Server} is offline", _configuration.ElasticSearch.Url);
            Environment.Exit(1);
        }

        _logger.LogInformation("Elasticsearch connection to server {Server} is online", _configuration.ElasticSearch.Url);
    }
}
