namespace Zilean.Shared.Features.ElasticSearch;

public interface IElasticClient
{
    ElasticsearchClient GetClient();
    Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, CancellationToken cancellationToken, int batchSize = 5000) where T : class;
}

public class ElasticClient : IElasticClient
{
    private readonly ILogger<ElasticClient> _logger;
    public const string DmmIndex = "dmm-entries";

    private readonly ElasticsearchClient _client;

    public ElasticClient(ZileanConfiguration configuration, ILogger<ElasticClient> logger)
    {
        _logger = logger;
        var nodes = new Uri[]
        {
            new(configuration.ElasticSearch.Url)
        };

        var pool = new StaticNodePool(nodes);

        var settings = new ElasticsearchClientSettings(pool)
            .DefaultMappingFor<ExtractedDmmEntry>(x =>
            {
                x.IndexName(DmmIndex);
                x.IdProperty(p => p.InfoHash);
            });

        _client = new ElasticsearchClient(settings);

        _logger.LogInformation("Checking Elasticsearch connection to server {Server}", configuration.ElasticSearch.Url);

        var isOnline = _client.PingAsync().GetAwaiter().GetResult();

        if (!isOnline.IsSuccess())
        {
            _logger.LogError("Elasticsearch connection to server {Server} is offline", configuration.ElasticSearch.Url);
            Environment.Exit(1);
        }

        _logger.LogInformation("Elasticsearch connection to server {Server} is online", configuration.ElasticSearch.Url);
    }

    public ElasticsearchClient GetClient() => _client;

    public async Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, CancellationToken cancellationToken,
        int batchSize = 5000) where T : class
    {
        List<BulkResponse> responses = [];

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, stopping indexing");
                break;
            }

            var batch = documents.GetRange(i, Math.Min(batchSize, documents.Count - i));
            var response = await BulkIndex(batch, index, cancellationToken);
            responses.Add(response);

            if (!response.IsSuccess())
            {
                _logger.LogError("Failed to index batch {Batch} of {Total} documents", i, documents.Count);
                _logger.LogError("Error: {Error}", response.ApiCallDetails.OriginalException.Message);
                break;
            }
        }

        return new BulkResponse
        {
            Took = responses.Sum(x => x.Took),
            Errors = responses.Any(x => x.Errors),
            Items = responses.SelectMany(x => x.Items).ToList(),
            IngestTook = responses.Sum(x => x.IngestTook),
        };
    }

    private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index, CancellationToken cancellationToken) where T : class => _client.IndexManyAsync(batch, index, cancellationToken: cancellationToken);
}
