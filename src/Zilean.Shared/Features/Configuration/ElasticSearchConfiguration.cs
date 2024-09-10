namespace Zilean.Shared.Features.Configuration;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = "Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=300;CommandTimeout=300;";
}
