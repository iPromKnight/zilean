namespace Zilean.Shared.Features.Blacklist;

public class BlacklistedItem
{
    [JsonPropertyName("info_hash")]
    public string? InfoHash { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("blacklisted_at")]
    public DateTime? BlacklistedAt { get; set; }
}
