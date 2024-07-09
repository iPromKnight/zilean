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

    private static async Task<Ok<ExtractedDmmEntry[]>> PerformSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
            }

            var client = await elasticClient.GetClient();

            var results = await client.SearchAsync<TorrentInfo>(s => s
                .Index(ElasticSearchClient.DmmIndex)
                .From(0)
                .Size(configuration.Dmm.MaxFilteredResults)
                .Query(DmmFilteredQueries.PerformUnfilteredSearch(queryRequest)));

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry>())
                : TypedResults.Ok(results.Hits.Where(x => x.Score >= configuration.Dmm.MinimumScoreMatch).Select(x => x.Source.ToExtractedDmmEntry()).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
        }
    }

    private static async Task<Ok<TorrentInfo[]>> PerformFilteredSearch(HttpContext context, IElasticSearchClient elasticClient, ZileanConfiguration configuration, [AsParameters] DmmFilteredRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return TypedResults.Ok(Array.Empty<TorrentInfo>());
            }

            var client = await elasticClient.GetClient();

            var results = await client
                .SearchAsync<TorrentInfo>(s => s
                    .Index(ElasticSearchClient.DmmIndex)
                    .From(0)
                    .Size(configuration.Dmm.MaxFilteredResults)
                    .Query(DmmFilteredQueries.PerformElasticSearchFiltered(request))

                );

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<TorrentInfo>())
                : TypedResults.Ok(results.Hits.Where(x=> x.Score >= configuration.Dmm.MinimumScoreMatch).Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<TorrentInfo>());
        }
    }
}
