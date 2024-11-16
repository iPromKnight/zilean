namespace Zilean.Scraper.Features.Bootstrapping;

public class EnsureMigrated(ImdbMetadataLoader metadataLoader, ILogger<EnsureMigrated> logger, ZileanDbContext dbContext, ZileanConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying Migrations...");
        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Migrations Applied.");

        if (configuration.Imdb.EnableImportMatching)
        {
            var imdbLoadedResult = await metadataLoader.Execute(cancellationToken);

            if (imdbLoadedResult == 1)
            {
                Environment.ExitCode = 1;
                Process.GetCurrentProcess().Kill();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
