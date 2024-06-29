namespace Zilean.ApiService.Features.Dmm;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDmmSupport(this IServiceCollection services)
    {
        services.AddHttpClient<IDMMFileDownloader, DMMFileDownloader>(DMMFileDownloader.ClientName, client =>
        {
            client.BaseAddress = new("https://github.com/debridmediamanager/hashlists/zipball/main/");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("curl");
        });

        services.AddSingleton<IDebridMediaManagerCrawler, DebridMediaManagerCrawler>();
        services.AddTransient<DmmScheduledJob>();
        services.AddExamine(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "lucene")));
        services.AddExamineLuceneIndex("DMM");
        services.AddScheduler();
        services.AddHostedService<DmmRunOnStartupService>();

        return services;
    }

    public static IServiceProvider ScheduleDmmJobs(this IServiceProvider provider)
    {
        provider.UseScheduler(scheduler =>
            {
                scheduler.Schedule<DmmScheduledJob>()
                    .Hourly()
                    .PreventOverlapping(nameof(DmmScheduledJob));
            })
            .LogScheduledTaskProgress(provider.GetService<ILogger<IScheduler>>());

        return provider;
    }
}
