using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Zilean.Scraper.Features.Ingestion.Processing;

public static class TorrentInfoExtensions
{
    public static bool IsBlacklisted(this TorrentInfo torrent, HashSet<string> blacklistedItems) =>
        blacklistedItems.Any(x => x.Equals(torrent.InfoHash, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<TorrentInfo> FilterOutTrashTorrents(this IEnumerable<TorrentInfo> finalizedTorrentsEnumerable,
        List<TorrentInfo> parsedTorrents, ZileanConfiguration configuration, ILogger logger, ProcessedCounts processedCount)
    {
        if (configuration.Parsing.IncludeTrash)
        {
            return finalizedTorrentsEnumerable;
        }

        finalizedTorrentsEnumerable = finalizedTorrentsEnumerable.Where(t => !t.Trash == true);
        var trashCount = parsedTorrents.Count(x => x.Trash == true);
        logger.LogInformation("Filtered out {Count} trash torrents", trashCount);
        processedCount.AddTrashRemoved(trashCount);

        return finalizedTorrentsEnumerable;
    }

    public static IEnumerable<TorrentInfo> FilterAdultTorrents(this IEnumerable<TorrentInfo> finalizedTorrentsEnumerable,
        List<TorrentInfo> parsedTorrents, ZileanConfiguration configuration, ILogger logger, ProcessedCounts processedCount)
    {
        if (configuration.Parsing.IncludeAdult)
        {
            return finalizedTorrentsEnumerable;
        }

        finalizedTorrentsEnumerable = finalizedTorrentsEnumerable.Where(t => !t.IsAdult);
        var adultCount = parsedTorrents.Count(x => x.IsAdult);
        logger.LogInformation("Filtered out {Count} adult torrents", adultCount);
        processedCount.AddAdultRemoved(adultCount);

        return finalizedTorrentsEnumerable;
    }

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
