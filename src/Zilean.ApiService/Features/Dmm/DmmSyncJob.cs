namespace Zilean.ApiService.Features.Dmm;

public class DmmSyncJob(IShellExecutionService shellExecutionService, ILogger<DmmSyncJob> logger) : IInvocable, ICancellableInvocable
{
    public static string ParsedPagesFile => Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");

    public CancellationToken CancellationToken { get; set; }
    public CancellationToken Token { get; set; }

    public async Task Invoke()
    {
        logger.LogInformation("DmmSyncJob started");

        await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = Path.Combine(AppContext.BaseDirectory, "dmmscraper"),
            ShowOutput = true,
            CancellationToken = CancellationToken
        });

        logger.LogInformation("DmmSyncJob completed");
    }

    public static bool ShouldRunOnStartup() => !File.Exists(ParsedPagesFile);
}
