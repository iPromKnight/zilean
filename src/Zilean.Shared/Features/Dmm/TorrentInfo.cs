namespace Zilean.Shared.Features.Dmm;

public class TorrentInfo
{
    [JsonPropertyName("raw_title")]
    public string? RawTitle { get; set; }

    [JsonPropertyName("parsed_title")]
    public string? ParsedTitle { get; set; }

    [JsonPropertyName("normalized_title")]
    public string? NormalizedTitle { get; set; }

    [JsonPropertyName("cleaned_parsed_title")]
    public string? CleanedParsedTitle { get; set; }

    [JsonPropertyName("trash")]
    public bool? Trash { get; set; } = false;

    [JsonPropertyName("year")]
    public int? Year { get; set; } = 0;

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("seasons")]
    public int[]? Seasons { get; set; } = [];

    [JsonPropertyName("episodes")]
    public int[]? Episodes { get; set; } = [];

    [JsonPropertyName("complete")]
    public bool? Complete { get; set; } = false;

    [JsonPropertyName("volumes")]
    public int[]? Volumes { get; set; } = [];

    [JsonPropertyName("languages")]
    public string[]? Languages { get; set; } = [];

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("hdr")]
    public string[]? Hdr { get; set; } = [];

    [JsonPropertyName("codec")]
    public string? Codec { get; set; }

    [JsonPropertyName("audio")]
    public string[]? Audio { get; set; } = [];

    [JsonPropertyName("channels")]
    public string[]? Channels { get; set; } = [];

    [JsonPropertyName("dubbed")]
    public bool? Dubbed { get; set; } = false;

    [JsonPropertyName("subbed")]
    public bool? Subbed { get; set; } = false;

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("edition")]
    public string? Edition { get; set; }

    [JsonPropertyName("bit_depth")]
    public string? BitDepth { get; set; }

    [JsonPropertyName("bitrate")]
    public string? Bitrate { get; set; }

    [JsonPropertyName("network")]
    public string? Network { get; set; }

    [JsonPropertyName("extended")]
    public bool? Extended { get; set; } = false;

    [JsonPropertyName("converted")]
    public bool? Converted { get; set; } = false;

    [JsonPropertyName("hardcoded")]
    public bool? Hardcoded { get; set; } = false;

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("ppv")]
    public bool? Ppv { get; set; } = false;

    [JsonPropertyName("_3d")]
    public bool? Is3d { get; set; } = false;

    [JsonPropertyName("site")]
    public string? Site { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("proper")]
    public bool? Proper { get; set; } = false;

    [JsonPropertyName("repack")]
    public bool? Repack { get; set; } = false;

    [JsonPropertyName("retail")]
    public bool? Retail { get; set; } = false;

    [JsonPropertyName("upscaled")]
    public bool? Upscaled { get; set; } = false;

    [JsonPropertyName("remastered")]
    public bool? Remastered { get; set; } = false;

    [JsonPropertyName("unrated")]
    public bool? Unrated { get; set; } = false;

    [JsonPropertyName("documentary")]
    public bool? Documentary { get; set; } = false;

    [JsonPropertyName("episode_code")]
    public string? EpisodeCode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    [JsonPropertyName("torrent")]
    public bool? Torrent { get; set; } = false;

    [JsonPropertyName("category")]
    public string Category { get; set; } = default!;

    [JsonPropertyName("imdb_id")]
    public string? ImdbId { get; set; }

    [JsonPropertyName("imdb")]
    public virtual ImdbFile? Imdb { get; set; }

    [JsonPropertyName("info_hash")]
    public string InfoHash { get; set; } = default!;

    [JsonPropertyName("ingested_at")]
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
}
