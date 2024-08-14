namespace Zilean.DmmScraper.Features.Bootstrapping;

public class ServiceLifetime(ImdbMetadataLoader metadataLoader, DmmScraping dmmScraper, IServiceProvider serviceProvider, ILogger<ServiceLifetime> logger) : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
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
        var imdbLoadedResult = await metadataLoader.Execute(cancellationToken);

        if (imdbLoadedResult == 1)
        {
            Environment.ExitCode = 1;
            Process.GetCurrentProcess().Kill();
            return;
        }

        var dmmScrapedResult = await dmmScraper.Execute(cancellationToken);

        Environment.ExitCode = dmmScrapedResult;
        Process.GetCurrentProcess().Kill();
    }
}
