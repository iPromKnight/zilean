using Zilean.ApiService.Features.Search;

namespace Zilean.ApiService.Features.Bootstrapping;

public static class WebApplicationExtensions
{
    public static WebApplication EnableSwagger(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }

    public static WebApplication MapZileanEndpoints(this WebApplication app, ZileanConfiguration configuration) =>
        app
            .MapDmmEndpoints(configuration)
            .MapImdbEndpoints(configuration)
            .MapTorznabEndpoints(configuration)
            .MapHealthCheckEndpoints();
}
