namespace Zilean.Shared.Features.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; }

    public DatabaseConfiguration()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            host = "postgres";
        }

        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        if (string.IsNullOrWhiteSpace(database))
        {
            database = "zilean";
        }

        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        if (string.IsNullOrWhiteSpace(username))
        {
            username = "postgres";
        }

        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "postgres";
        }

        ConnectionString = $"Host={host};Database={database};Username={username};Password={password};Include Error Detail=true;Timeout=30;CommandTimeout=3600;";
    }
}
