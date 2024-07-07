using System.Text.Json;

namespace Zilean.Shared.Features.Configuration;

public class ZileanConfiguration
{
    private static readonly JsonSerializerOptions? _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public DmmConfiguration Dmm { get; set; } = new();
    public ElasticSearchConfiguration ElasticSearch { get; set; } = new();
    public ProwlarrConfiguration Prowlarr { get; set; } = new();

    public static void EnsureExists()
    {
        var settingsFilePath = Path.Combine(AppContext.BaseDirectory, ConfigurationLiterals.ConfigurationFolder, ConfigurationLiterals.SettingsConfigFilename);
        if (!File.Exists(settingsFilePath))
        {
            File.WriteAllText(settingsFilePath, DefaultConfigurationContents());
        }
    }

    private static string DefaultConfigurationContents()
    {
        var mainSettings = new Dictionary<string, object>
        {
            [ConfigurationLiterals.MainSettingsSectionName] = new ZileanConfiguration(),
        };

        return JsonSerializer.Serialize(mainSettings, _jsonSerializerOptions);
    }
}
