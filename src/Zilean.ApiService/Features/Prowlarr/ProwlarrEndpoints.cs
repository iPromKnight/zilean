namespace Zilean.ApiService.Features.Prowlarr;

public static class ProwlarrEndpoints
{
    private const string GroupName = "prowlarr";
    private const string Search = "/indexer";

    public static WebApplication MapProwlarrEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Prowlarr.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Prowlarr()
                .DisableAntiforgery()
                .AllowAnonymous();
        }

        return app;
    }

    private static RouteGroupBuilder Prowlarr(this RouteGroupBuilder group)
    {
        group.MapGet(Search, PerformSearch)
            .Produces<ExtractedDmmEntry[]>();

        return group;
    }

    private static async Task<Ok<ExtractedDmmEntry[]>> PerformSearch(HttpContext context, IElasticSearchClient elasticClient, ILoggerFactory loggerFactory, [FromQuery] string query, [FromQuery] int? season = null, [FromQuery] int? episode = null)
    {
        try
        {
            var logger = loggerFactory.CreateLogger("ProwlarrIndexer");

            if (string.IsNullOrEmpty(query))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
            }

            var client = await elasticClient.GetClient();

            var results = await client
                .SearchAsync<ExtractedDmmEntry>(s => s
                .Index(ElasticSearchClient.DmmIndex)
                .From(0)
                .Size(10000)
                .Query(ProwlarrQueries.PerformElasticSearchForProwlarrEndpoint(query, season, episode))

            );

            logger.LogInformation("Search Query: {query} | Season: {season} | Episode: {episode} | Results: {results}", query, season, episode, results.Hits.Count);

            foreach (var hit in results.Hits)
            {
                logger.LogInformation("Hit: {Hit} | Score: {Score}", hit.Source.Filename, hit.Score);
            }

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry>())
                : TypedResults.Ok(results.Hits.Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
        }
    }
}
