using Npgsql;

namespace Zilean.Shared.Features.Configuration;

public class DatabaseConfiguration
{
  public string ConnectionString { get; set; }

  public DatabaseConfiguration()
  {
    var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    if (string.IsNullOrWhiteSpace(password))
    {
      throw new InvalidOperationException("Environment variable POSTGRES_PASSWORD is not set.");
    }

    ConnectionString = $"Host=postgres;Database=zilean;Username=postgres;Password={password};Include Error Detail=true;Timeout=30;CommandTimeout=3600;";
  }

  /// <summary>
  /// Returns true if the configured password is empty or a known insecure default.
  /// </summary>
  public bool HasInsecurePassword()
  {
    try
    {
      var parsed = new NpgsqlConnectionStringBuilder(ConnectionString);
      return string.IsNullOrEmpty(parsed.Password) ||
             string.Equals(parsed.Password, "postgres", StringComparison.OrdinalIgnoreCase);
    }
    catch
    {
      return false;
    }
  }
}
