namespace Zilean.ApiService.Features.Bootstrapping;

public static class WebApplicationExtensions
{
    public static WebApplication EnableSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }

    public static WebApplication MapZileanEndpoints(this WebApplication app, ZileanConfiguration configuration) =>
        app
            .MapDmmEndpoints(configuration)
            .MapImdbEndpoints(configuration)
            .MapHealthCheckEndpoints();
}
