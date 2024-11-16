namespace Zilean.Shared.Features.Scraping;

public class StreamedEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("size")]
    public required long Size { get; set; }

    [JsonPropertyName("hash")]
    public required string InfoHash { get; set; }

    public TorrentInfo? ParseResponse { get; set; }
}
