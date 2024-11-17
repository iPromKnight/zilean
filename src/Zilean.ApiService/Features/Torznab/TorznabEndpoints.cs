namespace Zilean.ApiService.Features.Torznab;

public static class TorznabEndpoints
{
    private const string GroupName = "torznab";

    public static WebApplication MapTorznabEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Torznab.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Torznab()
                .DisableAntiforgery()
                .AllowAnonymous();
        }

        return app;
    }

    private static RouteGroupBuilder Torznab(this RouteGroupBuilder group)
    {
        group.MapGet("/api", Api);
        return group;
    }

    private static async Task<IResult> Api(HttpContext context, ILogger<PerformSearchLogger> logger, ITorrentInfoService torrentInfoService, [AsParameters] TorznabRequest request)
    {
        logger.LogInformation("Processing request for Torznab API with request {@Request}", request);

        if (!ValidateAndPrepareQuery(request, logger, out var torznabQuery, out var errorResult))
        {
            return errorResult;
        }

        if (torznabQuery.QueryType.Equals("caps", StringComparison.OrdinalIgnoreCase))
        {
            return GetCapabilities();
        }

        try
        {
            torznabQuery.ValidateQueryAgainstCapabilities(logger);

            var results = await GetResultsForQuery(torznabQuery, torrentInfoService);

            var resultPage = new ResultPage
            {
                Releases = results,
            };

            logger.LogInformation("Returning {Count} results for query", results.Count);

            var xml = resultPage.ToXml(ChannelInfo.Link);
            return new XmlResult<string>(xml, StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while performing a search.");
            return NewErrorResponse(900, ex.Message);
        }
    }

    private static bool ValidateAndPrepareQuery(TorznabRequest request, ILogger<PerformSearchLogger> logger, out TorznabQuery? torznabQuery, out IResult? errorResult)
    {
        torznabQuery = null;
        errorResult = null;

        var converted = request.ToTorznabQuery();

        if (converted is not { } query)
        {
            errorResult = NewErrorResponse(900, "Invalid query conversion");
            return false;
        }

        torznabQuery = query;

        if (torznabQuery.Limit > TorznabCapabilities.LimitsMax)
        {
            logger.LogWarning("Requested limit exceeds maximum allowed.");
            errorResult = NewErrorResponse(900, $"Requested limit exceeds maximum allowed ({TorznabCapabilities.LimitsMax}).");
            return false;
        }

        if (torznabQuery.CanHandleQuery())
        {
            return true;
        }

        logger.LogWarning("Unsupported query type or parameters.");
        errorResult = NewErrorResponse(201, "Unsupported query type or parameters.");
        return false;
    }

    private static async Task<List<ReleaseInfo>> GetResultsForQuery(TorznabQuery? query, ITorrentInfoService torrentInfoService)
    {
        ArgumentNullException.ThrowIfNull(query);

        var filter = new TorrentInfoFilter
        {
            Query = query.SearchTerm,
            Season = query.Season,
            Episode = query.Episode,
            Year = query.Year,
            ImdbId = query.ImdbID,
        };

        var limit = query.Limit switch
        {
            0 => TorznabCapabilities.LimitsDefault,
            > 0 => query.Limit,
            _ => TorznabCapabilities.LimitsDefault
        };

        var torrentResults = await torrentInfoService.SearchForTorrentInfoFiltered(filter, limit);

        var results = torrentResults.Select(t => new ReleaseInfo
        {
            Title = t.RawTitle,
            Magnet = Parsing.GetMagnetLink(t.InfoHash),
            InfoHash = t.InfoHash,
            PublishDate = t.IngestedAt,
            Size = Parsing.GetBytes(t.Size),
            Category = t.Category.Equals("tvSeries", StringComparison.OrdinalIgnoreCase)
                ? [TorznabCategoryTypes.TV.Id]
                : [TorznabCategoryTypes.Movies.Id],
            Imdb = Parsing.GetImdbId(t.ImdbId),
        });

        if (query.IsMovieSearch)
        {
            results = results.Where(r => r.Category.Contains(TorznabCategoryTypes.Movies.Id));
        }
        else if (query.IsTVSearch)
        {
            results = results.Where(r => r.Category.Contains(TorznabCategoryTypes.TV.Id));
        }

        results = query.Limit switch
        {
            0 => results.Take(TorznabCapabilities.LimitsDefault),
            > 0 => results.Take(query.Limit),
            _ => results
        };

        return results.ToList();
    }


    private static XmlResult<string> GetCapabilities()
    {
        var capabilitiesXml = TorznabCapabilities.ToXml();
        return new XmlResult<string>(capabilitiesXml, StatusCodes.Status200OK);
    }

    private static XmlResult<string> NewErrorResponse(int code, string description, int statusCode = StatusCodes.Status400BadRequest)
    {
        var response = TorznabErrorResponse.Create(code, description);
        return new XmlResult<string>(response, statusCode);
    }

    private abstract class PerformSearchLogger;
}
