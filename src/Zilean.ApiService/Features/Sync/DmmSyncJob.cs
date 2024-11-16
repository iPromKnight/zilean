namespace Zilean.ApiService.Features.Sync;

public class DmmSyncJob(IShellExecutionService shellExecutionService, ILogger<DmmSyncJob> logger, ZileanDbContext dbContext) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }
    private const string DmmSyncArg = "dmm-sync";

    public async Task Invoke()
    {
        logger.LogInformation("Dmm SyncJob started");

        var argumentBuilder = ArgumentsBuilder.Create();
        argumentBuilder.AppendArgument(DmmSyncArg, string.Empty, false, false);

        await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = Path.Combine(AppContext.BaseDirectory, "scraper"),
            ArgumentsBuilder = argumentBuilder,
            ShowOutput = true,
            CancellationToken = CancellationToken
        });

        logger.LogInformation("Dmm SyncJob completed");
    }

    // ReSharper disable once MethodSupportsCancellation
    public Task<bool> ShouldRunOnStartup() => dbContext.ParsedPages.AnyAsync();
}
