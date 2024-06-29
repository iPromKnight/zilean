namespace Zilean.ApiService.Features.Bootstrapping;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediation(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.AddConsumers(typeof(Program).Assembly);
        });

        return services;
    }

    public static IServiceCollection AddSwaggerSupport(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Version = "v1",
                    Title = "Zilean API",
                    Description = "Arrless Searching for Riven",
                });
            });

        return services;
    }
}
