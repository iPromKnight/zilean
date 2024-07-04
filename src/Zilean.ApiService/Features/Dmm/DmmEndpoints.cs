namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    private const string GroupName = "dmm";
    private const string Search = "/search";

    public static WebApplication MapDmmEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Dmm.Enabled)
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

    private static async Task<Ok<ExtractedDmmEntry?[]>> PerformSearch(HttpContext context, IElasticClient elasticClient, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Array.Empty<ExtractedDmmEntry?>());
            }

            var results = await elasticClient
                .GetClient()
                .SearchAsync<ExtractedDmmEntry>(search =>
                {
                    search.Index(ElasticClient.DmmIndex)
                        .From(0)
                        .Size(1000)
                        .Query(q =>
                            q.Match(t =>
                                t.Field(f => f.Filename)
                                    .Query(queryRequest.QueryText)));
                });

            return !results.IsValidResponse || results.Hits.Count == 0
                ? TypedResults.Ok(Array.Empty<ExtractedDmmEntry?>())
                : TypedResults.Ok(results.Hits.Select(x => x.Source).ToArray());
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ExtractedDmmEntry?>());
        }
    }
}
