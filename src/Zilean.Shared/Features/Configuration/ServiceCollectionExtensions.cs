namespace Zilean.Shared.Features.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, ZileanConfiguration configuration)
    {
        services.AddSingleton(configuration);

        return services;
    }
}
