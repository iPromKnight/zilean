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

        BulkResponse finalResponse = null;

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, stopping indexing.");
                break;
            }

            var batch = documents.GetRange(i, Math.Min(batchSize, documents.Count - i));
            _logger.LogInformation("Indexing batch of {BatchSize} documents.", batch.Count);

            var response = await BulkIndex(batch, index, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index batch. Error: {Error}", response.OriginalException?.Message);
                return response;
            }

            finalResponse = response;
        }

        return finalResponse;
    }

    private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index, CancellationToken cancellationToken) where T : class =>
        _client.IndexManyAsync(batch, index, cancellationToken: cancellationToken);

    private static ElasticClient CreateNewClient(ZileanConfiguration configuration)
    {
        var pool = new SingleNodeConnectionPool(new Uri(configuration.ElasticSearch.Url));
        var settings = new ConnectionSettings(pool)
            .DefaultMappingFor<TorrentInfo>(TorrentInfo.TorrentInfoDefaultMapping)
            .EnableApiVersioningHeader();

        return new ElasticClient(settings);
    }

    private async Task CreateIndexForTorrentInfo()
    {
        var indexExists = await _client.Indices.ExistsAsync(DmmIndex);

        if (indexExists.Exists)
        {
            _logger.LogInformation("Index {Index} already exists. Skipping index creation.", DmmIndex);
            return;
        }

        var createIndexResponse = await _client.Indices.CreateAsync(DmmIndex, c => c
            .Map<TorrentInfo>(TorrentInfo.TorrentInfoIndexMapping)
            .Settings(settings =>
            {
                settings.NumberOfReplicas(0);
                settings.NumberOfShards(1);
                return settings;
            }));

        if (!createIndexResponse.IsValid)
        {
            _logger.LogError("Failed to create index {Index}. Error: {Error}", DmmIndex, createIndexResponse.OriginalException?.Message);
            Environment.Exit(1);
        }
    }

    private async Task AsyncInitialization()
    {
        _logger.LogInformation("Checking Elasticsearch connection to server {Server}.", _configuration.ElasticSearch.Url);

        var isOnline = await _client.PingAsync();

        if (!isOnline.IsValid)
        {
            _logger.LogError("Elasticsearch connection to server {Server} is offline.", _configuration.ElasticSearch.Url);
            Environment.Exit(1);
        }

        _logger.LogInformation("Elasticsearch connection to server {Server} is online.", _configuration.ElasticSearch.Url);

        await CreateIndexForTorrentInfo();
    }
}
