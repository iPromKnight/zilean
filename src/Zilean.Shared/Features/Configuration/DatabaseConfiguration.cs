using Npgsql;

namespace Zilean.Shared.Features.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; }

    public DatabaseConfiguration()
    {
        // Check for full connection string first (backwards compat with v3.5.0)
        var fullConnString = Environment.GetEnvironmentVariable("Zilean__Database__ConnectionString");
        if (!string.IsNullOrWhiteSpace(fullConnString))
        {
            ConnectionString = fullConnString;
            return;
        }

        // Build from individual env vars
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "zilean";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.Parse(port),
            Database = db,
            Username = user,
            Password = password,
            IncludeErrorDetail = true,
            Timeout = 30,
            CommandTimeout = 3600,
        };

        ConnectionString = builder.ConnectionString;
    }
}
