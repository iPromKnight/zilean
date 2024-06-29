namespace Zilean.ApiService.Features.HealthChecks;

public static class HealthCheckEndpoints
{
    public const string GroupName = "healthchecks";
    public const string Ping = "/ping";

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

        return group;
    }

    private static string RespondPong(HttpContext context) => $"[{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}]: Pong!";
}
