namespace Zilean.ApiService.Features.Dmm;

public class DmmRunOnStartupService(IDebridMediaManagerCrawler dmmCrawler, ILogger<DmmRunOnStartupService> logger) : IHostedService
{
    private readonly string _parsedPageFile = Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(_parsedPageFile))
        {
            logger.LogInformation("Parsed pages file exists, skipping initial run");
            return;
        }

        await dmmCrawler.Execute(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
