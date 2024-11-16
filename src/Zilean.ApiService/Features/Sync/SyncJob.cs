namespace Zilean.ApiService.Features.Sync;

public class SyncJob(IShellExecutionService shellExecutionService, ILogger<SyncJob> logger, ZileanDbContext dbContext) : IInvocable, ICancellableInvocable
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

    // ReSharper disable once MethodSupportsCancellation
    public Task<bool> ShouldRunOnStartup() => dbContext.ParsedPages.AnyAsync();
}
