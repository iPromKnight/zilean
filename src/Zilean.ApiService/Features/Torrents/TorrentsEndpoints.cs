namespace Zilean.ApiService.Features.Torrents;

public static class TorrentsEndpoints
{
    private const string GroupName = "torrents";
    private const string Scrape = "/all";
    private const string CheckCached = "/checkcached";
    private const string NoHashesProvidedError = "No hashes provided";
    private const string TooManyHashesError = "Too many hashes provided. The limit is {0}.";

    public static WebApplication MapTorrentsEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Torrents.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Torrents()
                .DisableAntiforgery()
                .RequireAuthorization(ApiKeyAuthentication.Policy)
                .WithMetadata(new OpenApiSecurityMetadata(ApiKeyAuthentication.Scheme));
        }

        return app;
    }

    private static RouteGroupBuilder Torrents(this RouteGroupBuilder group)
    {
        group.MapGet(Scrape, StreamTorrents)
            .Produces<StreamedEntry[]>()
            .AllowAnonymous();

        group.MapGet(CheckCached, CheckCachedTorrents)
            .Produces<CachedItem[]>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<BadRequest<ErrorResponse>>();

        return group;
    }

    private static async Task<IResult> CheckCachedTorrents(HttpContext context, ZileanDbContext dbContext, ILogger<CheckCachedRequest> logger, ZileanConfiguration configuration, [AsParameters] CheckCachedRequest request)
    {
        try
        {
            if (request.Hashes.IsNullOrWhiteSpace())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.BadRequest(new ErrorResponse(NoHashesProvidedError));
            }

            var hashes = request.Hashes.Split(',');
            if (hashes.Length >= configuration.Torrents.MaxHashesToCheck)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.BadRequest(new ErrorResponse(string.Format(TooManyHashesError, configuration.Torrents.MaxHashesToCheck)));
            }

            var hashSet = new HashSet<string>(hashes);

            var items = await dbContext
                .Torrents
                .Where(record => hashSet.Contains(record.InfoHash))
                .Select(record => new CachedItem
                {
                    InfoHash = record.InfoHash,
                    IsCached = true,
                    Item = record
                })
                .ToListAsync();

            foreach (var hash in hashSet.Where(hash => items.All(x => !x.InfoHash.Equals(hash, StringComparison.OrdinalIgnoreCase))))
            {
                items.Add(new CachedItem
                {
                    InfoHash = hash,
                    IsCached = false,
                    Item = null
                });
            }

            return Results.Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking for cached availability");
            return Results.Problem(ex.Message);
        }
    }

    private static async Task StreamTorrents(HttpContext context, ZileanDbContext dbContext, ILogger<StreamLogger> logger)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Starting to stream torrents to client: {Client}", context.Connection.RemoteIpAddress);

        try
        {
            var response = context.Response;
            response.ContentType = "application/json";
            await using var writer = new Utf8JsonWriter(response.Body);

            await response.Body.WriteAsync("["u8.ToArray());

            var firstItem = true;

            await foreach (var item in dbContext.Torrents
                               .Select(record => new StreamedEntry
                               {
                                   Name = record.RawTitle,
                                   InfoHash = record.InfoHash,
                                   Size = long.Parse(record.Size),
                               })
                               .AsAsyncEnumerable()
                               .WithCancellation(context.RequestAborted))
            {
                if (!firstItem)
                {
                    await response.Body.WriteAsync(","u8.ToArray());
                }

                firstItem = false;

                await JsonSerializer.SerializeAsync(response.Body, item);
                await writer.FlushAsync();
            }

            await response.Body.WriteAsync("]"u8.ToArray());

            logger.LogInformation("Finished streaming torrents to client: {Client} in {Elapsed}s",
                context.Connection.RemoteIpAddress, sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while streaming torrents to client: {Client}", context.Connection.RemoteIpAddress);
        }
    }

    private abstract class StreamLogger;
    private abstract class CheckCachedLogger;
}
