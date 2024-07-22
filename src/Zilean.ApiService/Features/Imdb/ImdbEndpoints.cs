namespace Zilean.ApiService.Features.Imdb;

public static class ImdbEndpoints
{
    private const string GroupName = "imdb";
    private const string Search = "/search";

    public static WebApplication MapImdbEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Imdb.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Imdb()
                .DisableAntiforgery()
                .AllowAnonymous();
        }

        return app;
    }

    private static RouteGroupBuilder Imdb(this RouteGroupBuilder group)
    {
        group.MapPost(Search, PerformSearch)
            .Produces<ImdbFile[]>();

        return group;
    }

    private static async Task<Ok<ImdbFile[]>> PerformSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, ILogger<ImdbFilteredInstance> logger, [AsParameters] ImdbFilteredRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return TypedResults.Ok(Array.Empty<ImdbFile>());
            }

            logger.LogInformation("Performing imdb search for {@Request}", request);

            var client = await elasticClient.GetClient();

            var results = await client
                .SearchAsync<ImdbFile>(s => s
                    .Index(ElasticSearchClient.ImdbMetadataIndex)
                    .From(0)
                    .Size(10)
                    .Query(ImdbFilteredQueries.PerformElasticSearchFiltered(request))
                );

            logger.LogInformation("Filtered imdb search for {QueryText} returned {Count} results", request.Query, results.Hits.Count);

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ImdbFile>())
                : TypedResults.Ok(results.Hits.Where(x=> x.Score >= configuration.Imdb.MinimumScoreMatch).Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ImdbFile>());
        }
    }

    private abstract class ImdbFilteredInstance;
}
