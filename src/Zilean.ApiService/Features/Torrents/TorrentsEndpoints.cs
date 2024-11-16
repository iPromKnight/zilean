namespace Zilean.ApiService.Features.Torrents;

public static class TorrentsEndpoints
{
    private const string GroupName = "torrents";
    private const string Scrape = "/all";

    public static WebApplication MapTorrentsEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.Torrents.EnableEndpoint)
        {
            app.MapGroup(GroupName)
                .WithTags(GroupName)
                .Torrents()
                .DisableAntiforgery()
                .AllowAnonymous();
        }

        return app;
    }

    private static RouteGroupBuilder Torrents(this RouteGroupBuilder group)
    {
        group.MapGet(Scrape, StreamTorrents);

        return group;
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
}
