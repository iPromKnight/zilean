// namespace Zilean.Shared.Features.ElasticSearch;
//
// public interface IElasticSearchClient
// {
//     Task<ElasticClient> GetClient();
// }
//
// public class ElasticSearchClient : IElasticSearchClient
// {
//     public const string DmmIndex = "dmm-entries";
//
//     private readonly ZileanConfiguration _configuration;
//     private readonly ILogger<ElasticSearchClient> _logger;
//     private readonly Task _initializationTask;
//     private readonly ElasticClient _client;
//
//     public ElasticSearchClient(ZileanConfiguration configuration, ILogger<ElasticSearchClient> logger)
//     {
//         _configuration = configuration;
//         _logger = logger;
//         _client = CreateNewClient(configuration);
//         _initializationTask = AsyncInitialization();
//     }
//
//     public async Task<ElasticClient> GetClient()
//     {
//         await _initializationTask;
//         return _client;
//     }
//
//     public async Task<BulkResponse> IndexTorrentsAsync(List<TorrentInfo> torrents, IImdbAugmentor imdbState, int batchSize = 5000, string? pipeline = null, CancellationToken cancellationToken = default)
//     {
//         await _initializationTask;
//
//         _logger.LogInformation("Indexing {Count} documents to index {Index}.", torrents.Count, DmmIndex);
//
//         if (!string.IsNullOrEmpty(pipeline))
//         {
//             _logger.LogInformation("Using pipeline {Pipeline} for indexing.", pipeline);
//         }
//
//         BulkResponse finalResponse = null;
//         int totalBatches = (int)Math.Ceiling((double)torrents.Count / batchSize);
//
//         for (int i = 0; i < torrents.Count; i += batchSize)
//         {
//             if (cancellationToken.IsCancellationRequested)
//             {
//                 _logger.LogInformation("Cancellation requested, stopping indexing.");
//                 break;
//             }
//
//             var batch = torrents.GetRange(i, Math.Min(batchSize, torrents.Count - i));
//             int currentBatch = (i / batchSize) + 1;
//
//             await imdbState.AugmentTorrentsWithImdbDataAsync(_configuration, batch, currentBatch, totalBatches);
//
//             var response = await BulkIndex(batch, DmmIndex, pipeline, cancellationToken);
//
//             if (!response.IsValid)
//             {
//                 _logger.LogError("Failed to index batch. Error: {Error}", response.OriginalException?.Message);
//                 return response;
//             }
//
//             finalResponse = response;
//         }
//
//         return finalResponse;
//     }
//
//     private Task<BulkResponse> BulkIndex<T>(List<T> batch, string index, string pipeline, CancellationToken cancellationToken) where T : class
//     {
//         var bulkDescriptor = new BulkDescriptor()
//             .Index(index)
//             .IndexMany(batch)
//             .Refresh(Refresh.True);
//
//         if (!string.IsNullOrEmpty(pipeline))
//         {
//             bulkDescriptor.Pipeline(pipeline);
//         }
//
//         return _client.BulkAsync(_ => bulkDescriptor, cancellationToken);
//     }
//
//     private static ElasticClient CreateNewClient(ZileanConfiguration configuration)
//     {
//         var pool = new SingleNodeConnectionPool(new Uri(configuration.ElasticSearch.Url));
//         var settings = new ConnectionSettings(pool)
//             .DefaultMappingFor<TorrentInfo>(TorrentInfo.TorrentInfoDefaultMapping)
//             .EnableApiVersioningHeader();
//
//         return new ElasticClient(settings);
//     }
//
//     private async Task CreateIndexForTorrentInfo()
//     {
//         var indexExists = await _client.Indices.ExistsAsync(DmmIndex);
//
//         if (indexExists.Exists)
//         {
//             _logger.LogInformation("Index {Index} already exists. Skipping index creation.", DmmIndex);
//             return;
//         }
//
//         var createIndexResponse = await _client.Indices.CreateAsync(DmmIndex, c => c
//             .Map<TorrentInfo>(TorrentInfo.TorrentInfoIndexMapping)
//             .Settings(settings =>
//             {
//                 settings.NumberOfReplicas(0);
//                 settings.NumberOfShards(1);
//                 return settings;
//             }));
//
//         if (!createIndexResponse.IsValid)
//         {
//             _logger.LogError("Failed to create index {Index}. Error: {Error}", DmmIndex, createIndexResponse.OriginalException?.Message);
//             Environment.Exit(1);
//         }
//     }
//
//     private async Task AsyncInitialization()
//     {
//         _logger.LogInformation("Checking Elasticsearch connection to server {Server}.", _configuration.ElasticSearch.Url);
//
//         var isOnline = await _client.PingAsync();
//
//         if (!isOnline.IsValid)
//         {
//             _logger.LogError("Elasticsearch connection to server {Server} is offline.", _configuration.ElasticSearch.Url);
//             Environment.Exit(1);
//         }
//
//         _logger.LogInformation("Elasticsearch connection to server {Server} is online.", _configuration.ElasticSearch.Url);
//
//         if (_configuration.Dmm.EnableScraping)
//         {
//             await CreateIndexForTorrentInfo();
//         }
//     }
// }
