namespace Zilean.ApiService.Features.Bootstrapping;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder configuration)
    {
        var configurationFolderPath = Path.Combine(AppContext.BaseDirectory, Literals.ConfigurationFolder);

        EnsureConfigurationDirectoryExists(configurationFolderPath);

        ZileanConfiguration.EnsureExists();

        configuration.SetBasePath(configurationFolderPath);
        configuration.AddLoggingConfiguration(configurationFolderPath);
        configuration.AddJsonFile(Literals.SettingsConfigFilename, false, false);
        configuration.AddEnvironmentVariables();

        return configuration;
    }

    public static ZileanConfiguration GetZileanConfiguration(this IConfiguration configuration) =>
        configuration.GetSection(Literals.MainSettingsSectionName).Get<ZileanConfiguration>();

    private static void EnsureConfigurationDirectoryExists(string configurationFolderPath)
    {
        if (!Directory.Exists(configurationFolderPath))
        {
            Directory.CreateDirectory(configurationFolderPath);
        }
    }
}
