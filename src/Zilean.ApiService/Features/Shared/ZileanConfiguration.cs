namespace Zilean.ApiService.Features.Shared;

public class ZileanConfiguration
{
    private static readonly JsonSerializerOptions? _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public DmmConfiguration Dmm { get; set; } = new();

    public static void EnsureExists()
    {
        var settingsFilePath = Path.Combine(AppContext.BaseDirectory, Literals.ConfigurationFolder, Literals.SettingsConfigFilename);
        if (!File.Exists(settingsFilePath))
        {
            File.WriteAllText(settingsFilePath, DefaultConfigurationContents());
        }
    }

    private static string DefaultConfigurationContents()
    {
        var mainSettings = new Dictionary<string, object>
        {
            [Literals.MainSettingsSectionName] = new ZileanConfiguration(),
        };

        return JsonSerializer.Serialize(mainSettings, _jsonSerializerOptions);
    }
}
