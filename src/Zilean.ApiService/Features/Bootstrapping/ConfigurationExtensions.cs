namespace Zilean.ApiService.Features.Bootstrapping;

public static class ConfigurationExtensions
{
    private const string ConfigurationFolder = "data";
    private const string LoggingConfig = "logging.json";
    private const string SettingsConfig = "settings.json";

    public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder configuration)
    {
        EnsureConfigFilesExist();

        configuration.SetBasePath(Path.Combine(AppContext.BaseDirectory, ConfigurationFolder));

        configuration.AddJsonFile(LoggingConfig, false, true);
        configuration.AddJsonFile(SettingsConfig, false, true);

        configuration.AddEnvironmentVariables();

        return configuration;
    }

    private static void EnsureConfigFilesExist()
    {
        var loggingPath = Path.Combine(AppContext.BaseDirectory, ConfigurationFolder, LoggingConfig);
        if (!File.Exists(loggingPath))
        {
            File.WriteAllText(loggingPath, DefaultLoggingContents);
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, ConfigurationFolder, SettingsConfig);
        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, DefaultSettingsContents);
        }
    }

    private const string DefaultLoggingContents =
        """
        {
          "Serilog": {
            "MinimumLevel": {
              "Default": "Information",
              "Override": {
                "Microsoft": "Warning",
                "System": "Warning",
                "System.Net.Http.HttpClient.Scraper.LogicalHandler": "Warning",
                "System.Net.Http.HttpClient.Scraper.ClientHandler": "Warning"
              }
            }
          }
        }
        """;

    private const string DefaultSettingsContents =
        """
        {
          "Zilean": {
            "Dmm": {
              "Enabled": true
            }
          }
        }
        """;
}
