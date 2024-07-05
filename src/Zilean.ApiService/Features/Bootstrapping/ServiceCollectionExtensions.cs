namespace Zilean.ApiService.Features.Bootstrapping;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
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

    public static IServiceCollection AddSchedulingSupport(this IServiceCollection services) =>
        services.AddScheduler();

    public static IServiceCollection AddElasticSearchSupport(this IServiceCollection services)
    {
        services.AddSingleton<IElasticClient, ElasticClient>();

        return services;
    }

    public static IServiceCollection AddConfiguration(this IServiceCollection services, ZileanConfiguration configuration)
    {
        services.AddSingleton(configuration);

        return services;
    }

    public static IServiceCollection AddDmmSupport(this IServiceCollection services, ZileanConfiguration configuration)
    {
        if (!configuration.Dmm.Enabled)
        {
            return services;
        }

        services.AddHttpClient<IDmmFileDownloader, DmmFileDownloader>(DmmFileDownloader.ClientName, client =>
        {
            client.BaseAddress = new Uri("https://github.com/debridmediamanager/hashlists/zipball/main/");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("curl/7.54");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.AddTransient<DmmSyncJob>();
        services.AddSingleton<DmmSyncState>();

        return services;
    }

    public static IServiceProvider SetupScheduling(this IServiceProvider provider, ZileanConfiguration configuration)
    {
        provider.UseScheduler(scheduler =>
            {
                if (configuration.Dmm.Enabled)
                {
                    scheduler.Schedule<DmmSyncJob>()
                        .Cron(configuration.Dmm.ScrapeSchedule)
                        .PreventOverlapping(nameof(DmmSyncJob));
                }
            })
            .LogScheduledTaskProgress(provider.GetService<ILogger<IScheduler>>());

        return provider;
    }
}
