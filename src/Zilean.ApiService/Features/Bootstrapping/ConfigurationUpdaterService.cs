namespace Zilean.ApiService.Features.Bootstrapping;

public class ConfigurationUpdaterService(ZileanConfiguration configuration, ILogger<ConfigurationUpdaterService> logger) : IHostedService
{
    private const string ResetApiKeyEnvVar = "ZILEAN__NEW__API__KEY";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        bool firstRun = configuration.FirstRun;

        if (firstRun)
        {
            configuration.FirstRun = false;
        }

        if (Environment.GetEnvironmentVariable(ResetApiKeyEnvVar) is "1" or "true")
        {
            configuration.ApiKey = ApiKey.Generate();
            logger.LogInformation("API Key regenerated:'{ApiKey}'", configuration.ApiKey);
            logger.LogInformation("Please keep this key safe and secure.");
        }

        var configurationFolderPath = Path.Combine(AppContext.BaseDirectory, ConfigurationLiterals.ConfigurationFolder);
        var configurationFilePath = Path.Combine(configurationFolderPath, ConfigurationLiterals.SettingsConfigFilename);

        var configWrapper = new Dictionary<string, object>
        {
            [ConfigurationLiterals.MainSettingsSectionName] = configuration,
        };

        await File.WriteAllTextAsync(configurationFilePath,
            JsonSerializer.Serialize(configWrapper,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = null,
                }), cancellationToken);

        if (firstRun)
        {
            logger.LogInformation("Zilean API Key: '{ApiKey}'", configuration.ApiKey);
            logger.LogInformation("Please keep this key safe and secure.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
