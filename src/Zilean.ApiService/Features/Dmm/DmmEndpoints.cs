namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    private const string GroupName = "dmm";
    private const string Search = "/search";

    public static WebApplication MapDmmEndpoints(this WebApplication app, IConfiguration configuration)
    {
        if (DmmConfiguration.IsDmmEnabled(configuration))
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

    private static async Task<Results<Ok<List<ExtractedDmmEntry>>, ProblemHttpResult>> PerformSearch(HttpContext context, DmmSyncState dmmState, IExamineManager examineManager, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (dmmState.IsRunning)
            {
                return TypedResults.Ok(new List<ExtractedDmmEntry>());
            }

            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(new List<ExtractedDmmEntry>());
            }

            if (!examineManager.TryGetIndex("DMM", out var dmmIndexer))
            {
                const string error = "Failed to get dmm lucene indexer, aborting...";
                Serilog.Log.Error(error);
                return TypedResults.Ok(new List<ExtractedDmmEntry>());
            }

            return await Task.Run(() =>
            {
                var searcher = dmmIndexer.Searcher;
                var query = searcher.CreateQuery();

                var results = query
                    .Field("Filename", queryRequest.QueryText)
                    .Execute()
                    .OrderByDescending(r => r.Score)
                    .Select(r => new ExtractedDmmEntry(r["Filename"], r.Id, long.Parse(r["Filesize"])))
                    .ToList();

                examineManager.Dispose();

                return TypedResults.Ok(results);
            });
        }
        catch (Exception e)
        {
            return TypedResults.Problem(e.Message);
        }
    }
}
