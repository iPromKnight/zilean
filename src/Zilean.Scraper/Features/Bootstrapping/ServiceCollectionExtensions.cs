using Zilean.Scraper.Features.Ingestion.Dmm;

namespace Zilean.Scraper.Features.Bootstrapping;

public static class ServiceCollectionExtensions
{
    public static void AddScrapers(this IServiceCollection services, IConfiguration configuration)
    {
        var zileanConfiguration = configuration.GetZileanConfiguration();

        services.AddHttpClient();
        services.AddSingleton(zileanConfiguration);
        services.AddImdbServices();
        services.AddDmmServices();
        services.AddGenericServices();
        services.AddZileanDataServices(zileanConfiguration);
        services.AddSingleton<ParseTorrentNameService>();
        services.AddHostedService<EnsureMigrated>();
    }

    private static void AddDmmServices(this IServiceCollection services)
    {
        services.AddSingleton<DmmFileDownloader>();
        services.AddSingleton<DmmScraping>();
        services.AddTransient<DmmService>();
    }

    private static void AddGenericServices(this IServiceCollection services)
    {
        services.AddSingleton<GenericIngestionScraping>();
        services.AddSingleton<KubernetesServiceDiscovery>();
    }

    private static void AddImdbServices(this IServiceCollection services)
    {
        services.AddSingleton<ImdbMetadataLoader>();
        services.AddSingleton<ImdbConfiguration>();
        services.AddSingleton<ImdbFileDownloader>();
        services.AddSingleton<ImdbFileProcessor>();
        services.AddSingleton<ImdbFileService>();
    }
}
