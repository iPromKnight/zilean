namespace Zilean.ApiService.Features.Blacklist;

public static class BlacklistEndpoints
{
    private const string GroupName = "blacklist";
    private const string Add = "/add";
    private const string Remove = "/remove";

    public static WebApplication MapBlacklistEndpoints(this WebApplication app)
    {
        app.MapGroup(GroupName)
            .WithTags(GroupName)
            .Torrents()
            .DisableAntiforgery()
            .RequireAuthorization(ApiKeyAuthentication.Policy)
            .WithMetadata(new OpenApiSecurityMetadata(ApiKeyAuthentication.Scheme));

        return app;
    }

    private static RouteGroupBuilder Torrents(this RouteGroupBuilder group)
    {
        group.MapPut(Add, AddBlacklistItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);

        group.MapDelete(Remove, RemoveBlacklistItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> RemoveBlacklistItem(HttpContext context, ZileanDbContext dbContext, ILogger<BlacklistLogger> logger, [FromQuery] string infoHash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(infoHash))
            {
                logger.LogWarning("Attempted to remove blacklisted item with empty info hash");
                return Results.BadRequest("InfoHash is required");
            }

            var item = await dbContext.BlacklistedItems.FirstOrDefaultAsync(x => x.InfoHash == infoHash);

            if (item == null)
            {
                logger.LogWarning("Attempted to remove non-existent blacklisted item {InfoHash}", infoHash);
                return Results.NotFound();
            }

            dbContext.BlacklistedItems.Remove(item);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Removed blacklisted item {InfoHash}", infoHash);

            return Results.NoContent();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while removing a blacklisted item");
            return Results.BadRequest("An error occurred while removing a blacklisted item");
        }
    }

    private static async Task<IResult> AddBlacklistItem(HttpContext context, ZileanDbContext dbContext, [AsParameters] BlacklistItemRequest request, ILogger<BlacklistLogger> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.info_hash))
            {
                return Results.BadRequest("info_hash is required");
            }

            if (string.IsNullOrWhiteSpace(request.reason))
            {
                return Results.BadRequest("reason is required");
            }

            if (await dbContext.BlacklistedItems.AnyAsync(x => x.InfoHash == request.info_hash))
            {
                return Results.Conflict("Item already blacklisted");
            }

            var blacklistedItem = new BlacklistedItem
            {
                InfoHash = request.info_hash,
                Reason = request.reason,
                BlacklistedAt = DateTime.UtcNow
            };

            dbContext.BlacklistedItems.Add(blacklistedItem);

            var torrentInfo = await dbContext.Torrents.FirstOrDefaultAsync(x => x.InfoHash == request.info_hash);

            if (torrentInfo != null)
            {
                dbContext.Torrents.Remove(torrentInfo);
                logger.LogInformation("Removed torrent {InfoHash} from database", request.info_hash);
            }

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while adding a blacklisted item");
            return Results.BadRequest("An error occurred while adding a blacklisted item");
        }
    }

    private abstract class BlacklistLogger;
}
