namespace Zilean.ApiService.Features.Bootstrapping;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerSupport(this IServiceCollection services) =>
        services.AddOpenApi("v2", options =>
        {
            options.AddDocumentTransformer<ApiKeyDocumentTransformer>();
        });

    public static IServiceCollection AddSchedulingSupport(this IServiceCollection services) =>
        services.AddScheduler();

    public static IServiceCollection AddStartupHostedServices(this IServiceCollection services) =>
        services.AddHostedService<StartupService>()
            .AddHostedService<ConfigurationUpdaterService>();

    public static IServiceCollection ConditionallyRegisterDmmJob(this IServiceCollection services,
        ZileanConfiguration configuration)
    {
        if (configuration.Dmm.EnableScraping)
        {
            services.AddTransient<DmmSyncJob>();
            services.AddTransient<GenericSyncJob>();
            services.AddSingleton<SyncOnDemandState>();
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
                        .PreventOverlapping("SyncJobs");
                }

                if (configuration.Ingestion.EnableScraping)
                {
                    scheduler.Schedule<GenericSyncJob>()
                        .Cron(configuration.Ingestion.ScrapeSchedule)
                        .PreventOverlapping("SyncJobs");
                }
            })
            .LogScheduledTaskProgress();

        return provider;
    }

    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "None";
                options.DefaultAuthenticateScheme = "None";
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthentication.Scheme, _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ApiKeyAuthentication.Policy, policy =>
            {
                policy.AuthenticationSchemes.Add(ApiKeyAuthentication.Scheme);
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
