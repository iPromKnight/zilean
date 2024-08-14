namespace Zilean.DmmScraper.Features.Bootstrapping;

public static class ServiceCollectionExtensions
{
    public static void AddDmmScraper(this IServiceCollection services, IConfiguration configuration)
    {
        var zileanConfiguration = configuration.GetZileanConfiguration();

        services.AddSingleton(zileanConfiguration);
        services.AddImdbServices();
        services.AddDmmServices();
        services.AddZileanDataServices(zileanConfiguration);
        services.AddSingleton<ParseTorrentNameService>();
        services.AddHostedService<ServiceLifetime>();
    }

    private static void AddDmmServices(this IServiceCollection services)
    {
        services.AddSingleton<DmmSyncState>();
        services.AddSingleton<DmmPageProcessor>();
        services.AddSingleton<DmmFileDownloader>();
        services.AddSingleton<DmmScraping>();
    }

    private static void AddImdbServices(this IServiceCollection services)
    {
        services.AddSingleton<ImdbFileService>();
        services.AddSingleton<ImdbMetadataLoader>();
        services.AddSingleton<ImdbConfiguration>();
        services.AddSingleton<ImdbFileDownloader>();
        services.AddSingleton<ImdbFileProcessor>();
    }
}
