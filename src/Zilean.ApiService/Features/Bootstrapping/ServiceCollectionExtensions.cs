using Zilean.Database;

namespace Zilean.ApiService.Features.Bootstrapping;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerSupport(this IServiceCollection services) =>
        services.AddOpenApi("v2");

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
            })
            .LogScheduledTaskProgress();

        return provider;
    }
}
