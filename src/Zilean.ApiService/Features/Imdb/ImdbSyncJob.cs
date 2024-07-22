namespace Zilean.ApiService.Features.Imdb;

public class ImdbSyncJob(IShellExecutionService shellExecutionService, ILogger<ImdbSyncJob> logger) : IInvocable, ICancellableInvocable
{
    public static string IngestedImdbData => Path.Combine(AppContext.BaseDirectory, "data", "ingestedImdbData.json");

    public CancellationToken CancellationToken { get; set; }
    public CancellationToken Token { get; set; }

    public async Task Invoke()
    {
        logger.LogInformation("ImdbSyncJob started");

        await shellExecutionService.ExecuteCommand(new ShellCommandOptions
        {
            Command = Path.Combine(AppContext.BaseDirectory, "imdbloader"),
            ShowOutput = true,
            CancellationToken = CancellationToken
        });

        logger.LogInformation("ImdbSyncJob completed");
    }

    public static bool ShouldRunOnStartup() => !File.Exists(IngestedImdbData);
}
