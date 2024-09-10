namespace Zilean.Shared.Features.Dmm;

public static class TorrentInfoExtensions
{
    public static string CacheKey(this TorrentInfo torrentInfo) =>
        $"{torrentInfo.ParsedTitle}-{torrentInfo.Category}-{torrentInfo.Year}";
}
