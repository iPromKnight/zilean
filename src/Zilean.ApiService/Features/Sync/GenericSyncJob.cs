namespace Zilean.ApiService.Features.Sync;

public class GenericSyncJob(IShellExecutionService shellExecutionService, ILogger<GenericSyncJob> logger, ZileanDbContext dbContext) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }
    private const string GenericSyncArg = "generic-sync";

    public async Task Invoke()
    {
        logger.LogInformation("Generic SyncJob started");

        var argumentBuilder = ArgumentsBuilder.Create();
        argumentBuilder.AppendArgument(GenericSyncArg, string.Empty, false, false);

        await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = Path.Combine(AppContext.BaseDirectory, "scraper"),
            ArgumentsBuilder = argumentBuilder,
            ShowOutput = true,
            CancellationToken = CancellationToken
        });

        logger.LogInformation("Generic SyncJob completed");
    }

    // ReSharper disable once MethodSupportsCancellation
    public Task<bool> ShouldRunOnStartup() => dbContext.ParsedPages.AnyAsync();
}
