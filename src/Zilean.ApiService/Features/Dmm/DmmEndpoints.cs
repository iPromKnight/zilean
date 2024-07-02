using Examine;

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
            .ProducesProblem(500)
            .Produces<IReadOnlyCollection<string>>();

        return group;
    }

    private static Results<Ok<IEnumerable<ExtractedDmmEntry>>, ProblemHttpResult> PerformSearch(HttpContext context, DmmSyncState dmmState, IExamineManager examineManager, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (dmmState.IsRunning)
            {
                return TypedResults.Ok(Enumerable.Empty<ExtractedDmmEntry>());
            }

            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Enumerable.Empty<ExtractedDmmEntry>());
            }

            if (!examineManager.TryGetIndex("DMM", out var dmmIndexer))
            {
                const string error = "Failed to get dmm lucene indexer, aborting...";
                Serilog.Log.Error(error);
                return TypedResults.Ok(Enumerable.Empty<ExtractedDmmEntry>());
            }

            var results = dmmIndexer.Searcher
                .CreateQuery()
                .Field("Filename", queryRequest.QueryText)
                .Execute()
                .Select(r => new ExtractedDmmEntry(r["Filename"], r.Id, long.Parse(r["Filesize"])));

            return TypedResults.Ok(results);
        }
        catch (Exception e)
        {
            return TypedResults.Problem(e.Message);
        }
    }
}
