namespace Zilean.Database.Services;

public abstract class BaseDapperService(ILogger<BaseDapperService> logger, ZileanConfiguration configuration)
{
    protected ZileanConfiguration Configuration { get; } = configuration;

    protected async Task ExecuteCommandAsync(Func<NpgsqlConnection, Task> operation, string taskMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(taskMessage);
            await using var connection = new NpgsqlConnection(Configuration.Database.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await operation(connection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing a command.");
            Process.GetCurrentProcess().Kill();
        }
    }

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
