using Serilog;
using ILogger = Serilog.ILogger;

namespace Zilean.ApiService.Features.Torznab;

public static class TorznabRequestExtensions
{
    private static ILogger Logger => Log.ForContext(typeof(TorznabRequestExtensions));

    public static TorznabQuery? ToTorznabQuery(this TorznabRequest request)
    {
        try
        {
            var query = new TorznabQuery
            {
                QueryType = "search",
                SearchTerm = request.q,
                ImdbID = request.imdbid,
            };
            if (request.t != null)
            {
                query.QueryType = request.t;
            }

            if (!string.IsNullOrWhiteSpace(request.season))
            {
                query.Season = Parsing.CoerceInt(request.season);
            }

            if (!string.IsNullOrWhiteSpace(request.ep))
            {
                query.Episode = Parsing.CoerceInt(request.ep);
            }

            if (!string.IsNullOrWhiteSpace(request.extended))
            {
                query.Extended = Parsing.CoerceInt(request.extended);
            }

            if (!string.IsNullOrWhiteSpace(request.limit))
            {
                query.Limit = Parsing.CoerceInt(request.limit);
            }

            if (!string.IsNullOrWhiteSpace(request.offset))
            {
                query.Offset = Parsing.CoerceInt(request.offset);
            }

            query.Categories = request.cat != null
                ? request.cat.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToArray()
                : query.QueryType switch
                {
                    "movie" when !string.IsNullOrWhiteSpace(request.imdbid) => [TorznabCategoryTypes.Movies.Id],
                    "tvSearch" when !string.IsNullOrWhiteSpace(request.imdbid) => [TorznabCategoryTypes.TV.Id],
                    "xxx" when !string.IsNullOrWhiteSpace(request.imdbid) => [TorznabCategoryTypes.XXX.Id],
                    _ => []
                };


            if (!string.IsNullOrWhiteSpace(request.season))
            {
                query.Season = int.Parse(request.season);
            }

            if (!string.IsNullOrWhiteSpace(request.year))
            {
                query.Year = int.Parse(request.year);
            }

            return query;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to convert TorznabRequest to TorznabQuery");
            return null;
        }
    }
}
