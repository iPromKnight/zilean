namespace Zilean.Shared.Features.Configuration;

public class ZileanConfiguration
{
    private static readonly JsonSerializerOptions? _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public string? ApiKey { get; set; } = Utilities.ApiKey.Generate();
    public bool FirstRun { get; set; } = true;
    public DmmConfiguration Dmm { get; set; } = new();
    public TorznabConfiguration Torznab { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
    public TorrentsConfiguration Torrents { get; set; } = new();
    public ImdbConfiguration Imdb { get; set; } = new();
    public IngestionConfiguration Ingestion { get; set; } = new();
    public ParsingConfiguration Parsing { get; set; } = new();

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
