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

    public static IServiceCollection ConditionallyRegisterDmmJob(this IServiceCollection services, ZileanConfiguration configuration)
    {
        if (configuration.Dmm.EnableScraping)
        {
            services.AddTransient<DmmSyncJob>();
        }

        return services;
    }

    public static IServiceProvider SetupScheduling(this IServiceProvider provider, ZileanConfiguration configuration)
    {
        provider.UseScheduler(scheduler =>
            {
                if (configuration.Dmm.EnableScraping)
                {
                    var dmmSchedule = scheduler.Schedule<DmmSyncJob>()
                        .Cron(configuration.Dmm.ScrapeSchedule)
                        .PreventOverlapping(nameof(DmmSyncJob));

                    if (DmmSyncJob.ShouldRunOnStartup())
                    {
                        dmmSchedule.RunOnceAtStart();
                    }
                }
            })
            .LogScheduledTaskProgress(provider.GetService<ILogger<IScheduler>>());

        return provider;
    }
}
