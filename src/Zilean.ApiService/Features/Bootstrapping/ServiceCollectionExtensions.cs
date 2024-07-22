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

    public static IServiceCollection AddDataBootStrapping(this IServiceCollection services) =>
        services.AddHostedService<BootstrapIndexesService>();

    public static IServiceCollection ConditionallyRegisterDmmJob(this IServiceCollection services, ZileanConfiguration configuration)
    {
        if (configuration.Dmm.EnableScraping)
        {
            services.AddTransient<DmmSyncJob>();
            services.AddSingleton<DmmSyncOnDemandState>();
        }

        if (configuration.Imdb.EnableScraping)
        {
            services.AddTransient<ImdbSyncJob>();
        }

        return services;
    }

    public static IServiceProvider SetupScheduling(this IServiceProvider provider, ZileanConfiguration configuration)
    {
        provider.UseScheduler(scheduler =>
            {
                if (configuration.Dmm.EnableScraping)
                {
                    scheduler.Schedule<DmmSyncJob>()
                        .Cron(configuration.Dmm.ScrapeSchedule)
                        .PreventOverlapping(nameof(DmmSyncJob));
                }

                if (configuration.Imdb.EnableScraping)
                {
                    scheduler.Schedule<ImdbSyncJob>()
                        .Cron(configuration.Imdb.ScrapeSchedule)
                        .PreventOverlapping(nameof(ImdbSyncJob));
                }
            })
            .LogScheduledTaskProgress(provider.GetService<ILogger<IScheduler>>());

        return provider;
    }
}
