namespace Zilean.ApiService.Features.Logging;

public static class LoggingConfiguration
{
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

    public static IConfigurationBuilder AddLoggingConfiguration(this IConfigurationBuilder configuration, string configurationFolderPath)
    {
        EnsureExists(configurationFolderPath);

        configuration.AddJsonFile(Literals.LoggingConfigFilename, false, false);

        return configuration;
    }

    private static void EnsureExists(string configurationFolderPath)
    {
        var loggingPath = Path.Combine(configurationFolderPath, Literals.LoggingConfigFilename);
        if (!File.Exists(loggingPath))
        {
            File.WriteAllText(loggingPath, DefaultLoggingContents);
        }
    }
}
