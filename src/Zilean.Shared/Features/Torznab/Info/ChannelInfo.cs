namespace Zilean.Shared.Features.Torznab.Info;

public class ChannelInfo
{
    public const string GitHubRepo = "https://github.com/iPromKnight/zilean";
    public const string Title = "Zilean Indexer";
    public const string Description = "DMM Cached RD Indexer";
    public const string Language = "en-US";
    public const string Category = "search";
    public static Uri Link => new(GitHubRepo);
    public static ChannelInfo ZileanIndexer => new();
}
