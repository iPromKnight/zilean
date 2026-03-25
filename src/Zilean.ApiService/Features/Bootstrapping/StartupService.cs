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

        // Security check — warn about insecure Postgres credentials
        if (configuration.Database.HasInsecurePassword())
        {
            logger.LogWarning("SECURITY WARNING: PostgreSQL password is empty or set to the default 'postgres'. " +
                "This is a security risk — if your database port is exposed, attackers can connect and compromise your system. " +
                "Set a strong password via POSTGRES_PASSWORD or Zilean__Database__ConnectionString.");
        }

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
        var logger = loggerFactory.CreateLogger<StartupService>();

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

        logger.LogInformation("Zilean Running: Startup Complete.");
    }
}
