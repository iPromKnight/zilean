namespace Zilean.ApiService.Features.Dmm;

public class DmmRunOnStartupService(ILogger<DmmRunOnStartupService> logger, IServiceProvider serviceProvider) : IHostedService
{
    private readonly string _parsedPageFile = Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(_parsedPageFile))
        {
            logger.LogInformation("Parsed pages file exists, skipping initial run");
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var syncJob = scope.ServiceProvider.GetRequiredService<DmmSyncJob>();
        syncJob.CancellationToken = cancellationToken;
        await syncJob.Invoke();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
