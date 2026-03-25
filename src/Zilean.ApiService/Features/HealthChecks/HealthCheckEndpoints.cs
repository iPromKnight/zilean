using Npgsql;

namespace Zilean.ApiService.Features.HealthChecks;

public static class HealthCheckEndpoints
{
    private const string GroupName = "healthchecks";
    private const string Ping = "/ping";
    private const string Ready = "/ready";

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapGroup(GroupName)
            .WithTags(GroupName)
            .HealthChecks()
            .DisableAntiforgery()
            .AllowAnonymous();

        return app;
    }

    private static RouteGroupBuilder HealthChecks(this RouteGroupBuilder group)
    {
        group.MapGet(Ping, RespondPong);
        group.MapGet(Ready, CheckReady);

        return group;
    }

    private static string RespondPong(HttpContext context) => $"[{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}]: Pong!";

    private static async Task<IResult> CheckReady(ZileanConfiguration configuration)
    {
        try
        {
            await using var connection = new NpgsqlConnection(configuration.Database.ConnectionString);
            await connection.OpenAsync();
            return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return Results.Json(
                new { status = "unhealthy", error = ex.Message, timestamp = DateTime.UtcNow },
                statusCode: 503);
        }
    }
}
