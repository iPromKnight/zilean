namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    private const string GroupName = "dmm";
    private const string Search = "/search";
    private const string Filtered = "/filtered";
    private const string Ingest = "/on-demand-scrape";

    public static WebApplication MapDmmEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Dmm.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Dmm()
                .DisableAntiforgery()
                .AllowAnonymous();
        }

        return app;
    }

    private static RouteGroupBuilder Dmm(this RouteGroupBuilder group)
    {
        group.MapPost(Search, PerformSearch)
            .Produces<ExtractedDmmEntry[]>();

        group.MapGet(Filtered, PerformFilteredSearch)
            .Produces<ExtractedDmmEntry[]>();

        group.MapGet(Ingest, PerformOnDemandScrape);

        return group;
    }

    private static async Task PerformOnDemandScrape(HttpContext context, ILogger<GeneralInstance> logger, IShellExecutionService executionService, ILogger<DmmSyncJob> syncLogger, IMutex mutex, DmmSyncOnDemandState state)
    {
        if (state.IsRunning)
        {
            logger.LogWarning("On-demand scrape already running.");
            return;
        }

        logger.LogInformation("Trying to schedule on-demand scrape with a 5 minute timeout on lock acquisition.");

        bool available = mutex.TryGetLock(nameof(DmmSyncJob), 1);

        if(available)
        {
            try
            {
                logger.LogInformation("On-demand scrape mutex lock acquired.");
                state.IsRunning = true;
                await new DmmSyncJob(executionService, syncLogger).Invoke();
            }
            finally
            {
                mutex.Release(nameof(DmmSyncJob));
                state.IsRunning = false;
            }

            return;
        }

        logger.LogWarning("Failed to acquire lock for on-demand scrape.");
    }

    private static async Task<Ok<ExtractedDmmEntry[]>> PerformSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, ILogger<DmmUnfilteredInstance> logger, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
            }

            logger.LogInformation("Performing unfiltered search for {QueryText}", queryRequest.QueryText);

            var client = await elasticClient.GetClient();

            var results = await client.SearchAsync<TorrentInfo>(s => s
                .Index(ElasticSearchClient.DmmIndex)
                .From(0)
                .Size(configuration.Dmm.MaxFilteredResults)
                .Query(DmmFilteredQueries.PerformUnfilteredSearch(queryRequest)));

            logger.LogInformation("Unfiltered search for {QueryText} returned {Count} results", queryRequest.QueryText, results.Hits.Count);

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry>())
                : TypedResults.Ok(results.Hits.Where(x => x.Score >= configuration.Dmm.MinimumScoreMatch).Select(x => x.Source.ToExtractedDmmEntry()).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
        }
    }

    private static async Task<Ok<TorrentInfo[]>> PerformFilteredSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, ILogger<DmmFilteredInstance> logger, [AsParameters] DmmFilteredRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return TypedResults.Ok(Array.Empty<TorrentInfo>());
            }

            logger.LogInformation("Performing filtered search for {@Request}", request);

            var client = await elasticClient.GetClient();

            var results = await client
                .SearchAsync<TorrentInfo>(s => s
                    .Index(ElasticSearchClient.DmmIndex)
                    .From(0)
                    .Size(configuration.Dmm.MaxFilteredResults)
                    .Query(DmmFilteredQueries.PerformElasticSearchFiltered(request))
                );

            logger.LogInformation("Filtered search for {QueryText} returned {Count} results", request.Query, results.Hits.Count);

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<TorrentInfo>())
                : TypedResults.Ok(results.Hits.Where(x=> x.Score >= configuration.Dmm.MinimumScoreMatch).Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<TorrentInfo>());
        }
    }

    private abstract class DmmUnfilteredInstance;
    private abstract class DmmFilteredInstance;
    private abstract class GeneralInstance;
}
