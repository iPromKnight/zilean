namespace Zilean.ApiService.Features.Bootstrapping;

public static class ConfigurationExtensions
{
    private const string ConfigurationFolder = "data";
    private const string LoggingConfig = "logging.json";
    private const string SettingsConfig = "settings.json";

    public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder configuration)
    {
        configuration.SetBasePath(Path.Combine(AppContext.BaseDirectory, ConfigurationFolder));

        configuration.AddJsonFile(LoggingConfig, false, true);
        configuration.AddJsonFile(SettingsConfig, false, true);

        configuration.AddEnvironmentVariables();

        return configuration;
    }
}
