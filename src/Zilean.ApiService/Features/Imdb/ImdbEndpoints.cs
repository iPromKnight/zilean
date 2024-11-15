using Zilean.Database.Dtos;

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
            .Produces<ImdbSearchResult[]>();

        return group;
    }

    private static async Task<Ok<ImdbSearchResult[]>> PerformSearch(HttpContext context, IImdbFileService imdbFileService, ZileanConfiguration configuration, ILogger<ImdbFilteredInstance> logger, [AsParameters] ImdbFilteredRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return TypedResults.Ok(Array.Empty<ImdbSearchResult>());
            }

            logger.LogInformation("Performing imdb search for {@Request}", request);

            var results = await imdbFileService.SearchForImdbIdAsync(request.Query, request.Year, request.Category);

            logger.LogInformation("Filtered imdb search for {QueryText} returned {Count} results", request.Query, results.Length);

            return results.Length == 0
                ? TypedResults.Ok(Array.Empty<ImdbSearchResult>())
                : TypedResults.Ok(results);
        }
        catch
        {
            return TypedResults.Ok(Array.Empty<ImdbSearchResult>());
        }
    }

    private abstract class ImdbFilteredInstance;
}
