namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    private const string GroupName = "dmm";
    private const string Search = "/search";
    private const string Filtered = "/filtered";

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

        return group;
    }

    private static async Task<Ok<ExtractedDmmEntry[]>> PerformSearch(HttpContext context, IElasticSearchClient elasticClient, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
            }

            var client = await elasticClient.GetClient();

            var results = await client.SearchAsync<ExtractedDmmEntry>(s => s
                .Index(ElasticSearchClient.DmmIndex)
                .From(0)
                .Size(10000)
                .Query(DmmFilteredQueries.PerformUnfilteredSearch(queryRequest)));

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry>())
                : TypedResults.Ok(results.Hits.Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
        }
    }

    private static async Task<Ok<ExtractedDmmEntry[]>> PerformFilteredSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, [FromQuery] string query, [FromQuery] int? season = null, [FromQuery] int? episode = null)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
            }

            var client = await elasticClient.GetClient();

            var results = await client
                .SearchAsync<ExtractedDmmEntry>(s => s
                    .Index(ElasticSearchClient.DmmIndex)
                    .From(0)
                    .Size(configuration.Dmm.MaxFilteredResults)
                    .Query(DmmFilteredQueries.PerformElasticSearchFiltered(query, season, episode))

                );

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
