namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    private const string GroupName = "dmm";
    private const string Search = "/search";

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
            .Produces<ExtractedDmmEntry?[]>();

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

            var results = await client.SearchAsync<ExtractedDmmEntry>(search => CreateSearch(queryRequest, search));

            return !results.IsValid || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry>())
                : TypedResults.Ok(results.Hits.Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry>());
        }
    }

    private static ISearchRequest CreateSearch(DmmQueryRequest queryRequest, SearchDescriptor<ExtractedDmmEntry> search) =>
        search.Index(ElasticSearchClient.DmmIndex)
            .From(0)
            .Size(1000)
            .Query(q =>
                q.Match(t =>
                    t.Field(f => f.Filename)
                        .Query(queryRequest.QueryText)));
}
