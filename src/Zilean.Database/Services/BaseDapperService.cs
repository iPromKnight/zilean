namespace Zilean.Database.Services;

public abstract class BaseDapperService(ILogger<BaseDapperService> logger, ZileanConfiguration configuration)
{
    protected ZileanConfiguration Configuration { get; } = configuration;

    protected async Task ExecuteCommandAsync(Func<NpgsqlConnection, Task> operation, string taskMessage,
        CancellationToken cancellationToken = default) => await AnsiConsole.Progress()
        .AutoRefresh(true)
        .AutoClear(true)
        .HideCompleted(true)
        .Columns([
            new TaskDescriptionColumn(),
            new SpinnerColumn()
        ])
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask($"[cyan]{taskMessage}[/]", new ProgressTaskSettings
            {
                MaxValue = double.NaN
            });

            try
            {
                task.IsIndeterminate();
                await using var connection = new NpgsqlConnection(Configuration.Database.ConnectionString);
                await connection.OpenAsync(cancellationToken);
                await operation(connection);
            }
            catch (Exception ex)
            {
                task.StopTask();
                AnsiConsole.MarkupLine("[red]Error: {0}[/]", ex.Message);
                Process.GetCurrentProcess().Kill();
            }
            finally
            {
                task.StopTask();
            }
        });

    protected async Task<TResult> ExecuteCommandAsync<TResult>(Func<NpgsqlConnection, Task<TResult>> operation,
        string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(Configuration.Database.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var result = await operation(connection);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, errorMessage);
            throw;
        }
    }
}
