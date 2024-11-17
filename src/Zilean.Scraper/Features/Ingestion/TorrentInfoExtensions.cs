using Zilean.Shared.Features.Blacklist;

namespace Zilean.Scraper.Features.Ingestion;

public static class TorrentInfoExtensions
{
    public static bool WipeSomeTissue(this TorrentInfo torrent) =>
        !((torrent.RawTitle.Contains(" xxx ", StringComparison.OrdinalIgnoreCase) ||
           torrent.RawTitle.Contains(" xx ", StringComparison.OrdinalIgnoreCase)) &&
          !torrent.ParsedTitle.Contains("XXX", StringComparison.OrdinalIgnoreCase));

    public static bool IsBlacklisted(this TorrentInfo torrent, HashSet<string> blacklistedItems) =>
        blacklistedItems.Any(x => x.Equals(torrent.InfoHash, StringComparison.OrdinalIgnoreCase));
}
