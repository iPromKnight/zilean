namespace Zilean.ApiService.Features.Bootstrapping;

public class BootstrapIndexesService(
    ZileanConfiguration configuration,
    IShellExecutionService executionService,
    ILoggerFactory loggerFactory) : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (configuration.Imdb.EnableScraping && ImdbSyncJob.ShouldRunOnStartup())
        {
            var imdbJob = new ImdbSyncJob(executionService, loggerFactory.CreateLogger<ImdbSyncJob>());
            await imdbJob.Invoke();
        }

        if (configuration.Dmm.EnableScraping && DmmSyncJob.ShouldRunOnStartup())
        {
            var dmmJob = new DmmSyncJob(executionService, loggerFactory.CreateLogger<DmmSyncJob>());
            await dmmJob.Invoke();
        }
    }
}
