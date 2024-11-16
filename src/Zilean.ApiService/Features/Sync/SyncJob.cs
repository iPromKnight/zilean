namespace Zilean.ApiService.Features.Sync;

public class SyncJob(IShellExecutionService shellExecutionService, ILogger<SyncJob> logger, ITorrentInfoService infoService) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    public async Task Invoke()
    {
        logger.LogInformation("SyncJob started");

        await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = Path.Combine(AppContext.BaseDirectory, "scraper"),
            ShowOutput = true,
            CancellationToken = CancellationToken
        });

        logger.LogInformation("SyncJob completed");
    }

    public Task<bool> ShouldRunOnStartup() => infoService.HasParsedPages();
}
