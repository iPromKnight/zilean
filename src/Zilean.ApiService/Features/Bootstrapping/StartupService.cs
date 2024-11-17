namespace Zilean.ApiService.Features.Bootstrapping;

public class StartupService(
    ZileanConfiguration configuration,
    IShellExecutionService executionService,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<StartupService>();
        logger.LogInformation("Applying Migrations...");
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        var dbContext = asyncScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations Applied.");
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (configuration.Dmm.EnableScraping)
        {
            await using var asyncScope = serviceProvider.CreateAsyncScope();
            var dbContext = asyncScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
            var dmmJob = new DmmSyncJob(executionService, loggerFactory.CreateLogger<DmmSyncJob>(), dbContext);
            var pagesExist = await dmmJob.ShouldRunOnStartup();
            if (!pagesExist)
            {
                await dmmJob.Invoke();
            }
        }
    }
}
