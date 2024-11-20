namespace Zilean.Shared.Features.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = "Host=postgres;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=30;CommandTimeout=3600;";
}
