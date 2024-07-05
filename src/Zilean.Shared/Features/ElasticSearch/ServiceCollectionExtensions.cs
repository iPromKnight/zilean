namespace Zilean.Shared.Features.ElasticSearch;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElasticSearchSupport(this IServiceCollection services)
    {
        services.AddSingleton<IElasticClient, ElasticClient>();

        return services;
    }
}
