using Microsoft.Extensions.Configuration;

namespace Zilean.Shared.Features.Configuration;

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
                "System.Net.Http.HttpClient.Scraper.ClientHandler": "Warning",
                "Microsoft.AspNetCore.Hosting.Diagnostics": "Error",
                "Microsoft.AspNetCore.DataProtection": "Error",
              }
            }
          }
        }
        """;

    public static IConfigurationBuilder AddLoggingConfiguration(this IConfigurationBuilder configuration, string configurationFolderPath)
    {
        EnsureExists(configurationFolderPath);

        configuration.AddJsonFile(ConfigurationLiterals.LoggingConfigFilename, false, false);

        return configuration;
    }

    private static void EnsureExists(string configurationFolderPath)
    {
        var loggingPath = Path.Combine(configurationFolderPath, ConfigurationLiterals.LoggingConfigFilename);
        File.WriteAllText(loggingPath, DefaultLoggingContents);
    }
}
