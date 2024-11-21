using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Zilean.Scraper.Features.Ingestion.Processing;

public static class TorrentInfoExtensions
{
    public static bool IsBlacklisted(this TorrentInfo torrent, HashSet<string> blacklistedItems) =>
        blacklistedItems.Any(x => x.Equals(torrent.InfoHash, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<TorrentInfo> FilterBlacklistedTorrents(this IEnumerable<TorrentInfo> finalizedTorrentsEnumerable,
        List<TorrentInfo> parsedTorrents, HashSet<string> blacklistedHashes, ZileanConfiguration configuration, ILogger logger,
        ProcessedCounts processedCount)
    {
        if (blacklistedHashes.Count <= 0)
        {
            return finalizedTorrentsEnumerable;
        }

        finalizedTorrentsEnumerable = finalizedTorrentsEnumerable.Where(t => !blacklistedHashes.Contains(t.InfoHash));
        var blacklistedCount = parsedTorrents.Count(x => blacklistedHashes.Contains(x.InfoHash));
        logger.LogInformation("Filtered out {Count} blacklisted torrents", blacklistedCount);
        processedCount.AddBlacklistedRemoved(blacklistedCount);

        return finalizedTorrentsEnumerable;
    }
}
