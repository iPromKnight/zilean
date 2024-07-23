namespace Zilean.Shared.Features.ElasticSearch;

public interface IElasticSearchClient
{
    Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, int batchSize = 5000, string? pipeline = null, CancellationToken cancellationToken = default) where T : class;
    Task<ElasticClient> GetClient();
}

public class ElasticSearchClient : IElasticSearchClient
{
    public const string DmmIndex = "dmm-entries";
    public const string ImdbMetadataIndex = "imdbmeta-entries";

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

    public async Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, int batchSize = 1000, string? pipeline = null, CancellationToken cancellationToken = default) where T : class
    {
        await _initializationTask;

        _logger.LogInformation("Indexing {Count} documents to index {Index}.", documents.Count, index);

        BulkResponse finalResponse = null;

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, stopping indexing.");
                break;
            }

            var batch = documents.GetRange(i, Math.Min(batchSize, documents.Count - i));

            var response = await BulkIndex(batch, index, pipeline, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index batch. Error: {Error}", response.OriginalException?.Message);
                return response;
            }

            finalResponse = response;
        }

        return finalResponse;
    }

    private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index, string pipeline, CancellationToken cancellationToken) where T : class =>
        !string.IsNullOrEmpty(pipeline)
            ? _client.BulkAsync(b => b
                    .Index(index)
                    .Pipeline(pipeline)
                    .IndexMany(batch)
                    .Refresh(Refresh.True)
                , cancellationToken)
            : _client.BulkAsync(b => b
                    .Index(index)
                    .IndexMany(batch)
                    .Refresh(Refresh.True)
                , cancellationToken);

    private static ElasticClient CreateNewClient(ZileanConfiguration configuration)
    {
        var pool = new SingleNodeConnectionPool(new Uri(configuration.ElasticSearch.Url));
        var settings = new ConnectionSettings(pool)
            .DefaultMappingFor<TorrentInfo>(TorrentInfo.TorrentInfoDefaultMapping)
            .DefaultMappingFor<ImdbFile>(ImdbIndexer.ImdbFileDefaultMapping)
            .EnableApiVersioningHeader();

#if DEBUG
        settings.SetupDebugMode();
#endif

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

    private async Task CreateIndexForImdbMetadata()
    {
        var indexExists = await _client.Indices.ExistsAsync(ImdbMetadataIndex);

        if (indexExists.Exists)
        {
            _logger.LogInformation("Index {Index} already exists. Skipping index creation.", ImdbMetadataIndex);
            return;
        }

        var pipelineSetupSuccessfully = await ImdbIndexer.SetupImdbPipeline(_client, _logger);

        if (!pipelineSetupSuccessfully)
        {
            _logger.LogError("Failed to setup imdb pipeline.");
            Environment.Exit(1);
        }

        _logger.LogInformation("Successfully setup imdb processing pipeline.");
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

        if (_configuration.Imdb.EnableScraping)
        {
            await CreateIndexForImdbMetadata();
        }

        if (_configuration.Dmm.EnableScraping)
        {
            await CreateIndexForTorrentInfo();
        }
    }
}
