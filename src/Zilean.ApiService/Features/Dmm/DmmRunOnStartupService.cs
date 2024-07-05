namespace Zilean.ApiService.Features.Dmm;

public interface IConditionallyRegisteredHostedService : IHostedService;

public class DmmRunOnStartupService(ILogger<DmmRunOnStartupService> logger, DmmSyncJob syncJob) : IConditionallyRegisteredHostedService
{
    public static void ConditionallyRegister(IServiceCollection services, ZileanConfiguration configuration)
    {
        if (!configuration.Dmm.Enabled)
        {
            return;
        }

        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json")))
        {
            services.AddHostedService<DmmRunOnStartupService>();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        syncJob.CancellationToken = cancellationToken;
        await syncJob.Invoke();

        logger.LogInformation("Initial run completed. Cycling application in 5 seconds to free resources used by lucene indexers initial processing.");
        await Task.Delay(5000, cancellationToken);
        Environment.Exit(1);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
