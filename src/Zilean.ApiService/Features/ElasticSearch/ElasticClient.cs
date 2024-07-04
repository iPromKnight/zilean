namespace Zilean.ApiService.Features.ElasticSearch;

public interface IElasticClient
{
    ElasticsearchClient GetClient();
    Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, int batchSize = 5000) where T : class;
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

        var isOnline = _client.PingAsync().GetAwaiter().GetResult();

        if (!isOnline.IsSuccess())
        {
            throw new InvalidOperationException("Elasticsearch is not online, or client ping failed...");
        }
    }

    public ElasticsearchClient GetClient() => _client;

    public async Task<BulkResponse> IndexManyBatchedAsync<T>(List<T> documents, string index, int batchSize = 5000) where T : class
    {
        List<BulkResponse> responses = [];

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var batch = documents.GetRange(i, Math.Min(batchSize, documents.Count - i));
            var response = await BulkIndex(batch, index);
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

    private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index) where T : class => _client.IndexManyAsync(batch, index);
}
