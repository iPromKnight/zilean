namespace Zilean.ApiService.Features.Dmm;

public static class DmmEndpoints
{
    public const string GroupName = "dmm";
    public const string Search = "/search";

    public static WebApplication MapDmmEndpoints(this WebApplication app)
    {
        app.MapGroup(GroupName)
            .WithTags(GroupName)
            .Dmm()
            .DisableAntiforgery()
            .AllowAnonymous();

        return app;
    }

    private static RouteGroupBuilder Dmm(this RouteGroupBuilder group)
    {
        group.MapPost(Search, PerformSearch)
            .ProducesProblem(500)
            .Produces<IReadOnlyCollection<string>>();

        return group;
    }

    private static async Task<Results<Ok<IEnumerable<DebridMediaManagerCrawler.ExtractedDMMContent>>, ProblemHttpResult>> PerformSearch(HttpContext context, IExamineManager examineManager, IDebridMediaManagerCrawler dmmCrawler, [FromBody] DmmQueryRequest queryRequest)
    {
        try
        {
            if (dmmCrawler.IsRunning)
            {
                return TypedResults.Ok(Enumerable.Empty<DebridMediaManagerCrawler.ExtractedDMMContent>());
            }

            if (string.IsNullOrEmpty(queryRequest.QueryText))
            {
                return TypedResults.Ok(Enumerable.Empty<DebridMediaManagerCrawler.ExtractedDMMContent>());
            }

            if (!examineManager.TryGetIndex("DMM", out var dmmIndexer))
            {
                const string error = "Failed to get dmm lucene indexer, aborting...";
                Serilog.Log.Error(error);
                return TypedResults.Ok(Enumerable.Empty<DebridMediaManagerCrawler.ExtractedDMMContent>());
            }

            return await Task.Run(() =>
            {
                var searcher = dmmIndexer.Searcher;
                var query = searcher.CreateQuery();

                var results = query
                    .Field("Filename", queryRequest.QueryText)
                    .Execute()
                    .OrderByDescending(r => r.Score)
                    .Take(200)
                    .Select(r => new DebridMediaManagerCrawler.ExtractedDMMContent(r["Filename"], r.Id, long.Parse(r["Filesize"])));

                return TypedResults.Ok(results);
            });
        }
        catch (Exception e)
        {
            return TypedResults.Problem(e.Message);
        }
    }
}
