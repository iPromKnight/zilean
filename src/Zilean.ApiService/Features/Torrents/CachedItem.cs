namespace Zilean.ApiService.Features.Torrents;

public class CachedItem
{
    [JsonPropertyName("info_hash")]
    public string? InfoHash { get; set; }
    [JsonPropertyName("is_cached")]
    public bool? IsCached { get; set; }
    [JsonPropertyName("item")]
    public TorrentInfo? Item { get; set; }
}
