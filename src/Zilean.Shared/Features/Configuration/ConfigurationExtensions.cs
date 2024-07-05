using Microsoft.Extensions.Configuration;

namespace Zilean.Shared.Features.Configuration;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder configuration)
    {
        var configurationFolderPath = Path.Combine(AppContext.BaseDirectory, ConfigurationLiterals.ConfigurationFolder);

        EnsureConfigurationDirectoryExists(configurationFolderPath);

        ZileanConfiguration.EnsureExists();

        configuration.SetBasePath(configurationFolderPath);
        configuration.AddLoggingConfiguration(configurationFolderPath);
        configuration.AddJsonFile(ConfigurationLiterals.SettingsConfigFilename, false, false);
        configuration.AddEnvironmentVariables();

        return configuration;
    }

    public static ZileanConfiguration GetZileanConfiguration(this IConfiguration configuration) =>
        configuration.GetSection(ConfigurationLiterals.MainSettingsSectionName).Get<ZileanConfiguration>();

    private static void EnsureConfigurationDirectoryExists(string configurationFolderPath)
    {
        if (!Directory.Exists(configurationFolderPath))
        {
            Directory.CreateDirectory(configurationFolderPath);
        }
    }
}
