namespace Zilean.Shared.Features.Dmm;

public class TorrentInfo
{
    [JsonPropertyName("info_hash")]
    public string InfoHash { get; set; } = default!;

    [JsonPropertyName("resolution")]
    public string[]? Resolution { get; set; } = [];

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("remastered")]
    public bool? Remastered { get; set; }

    [JsonPropertyName("codec")]
    public string[]? Codec { get; set; } = [];

    [JsonPropertyName("audio")]
    public string[]? Audio { get; set; } = [];

    [JsonPropertyName("quality")]
    public string[]? Quality { get; set; } = [];

    [JsonPropertyName("episode")]
    public int[]? Episodes { get; set; } = [];

    [JsonPropertyName("season")]
    public int[]? Seasons { get; set; } = [];

    [JsonPropertyName("language")]
    public string[]? Languages { get; set; } = [];

    [JsonPropertyName("parsed_title")]
    public string? Title { get; set; }

    [JsonPropertyName("raw_title")]
    public string? RawTitle { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = default!;

    [JsonPropertyName("imdb_id")]
    public string? ImdbId { get; set; }

    [JsonPropertyName("imdb")]
    public virtual ImdbFile? Imdb { get; set; }
}
