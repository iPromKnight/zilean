namespace Zilean.Scraper.Features.Ingestion;

public static class TorrentInfoExtensions
{
    public static bool WipeSomeTissue(this TorrentInfo torrent) =>
        !((torrent.RawTitle.Contains(" xxx ", StringComparison.OrdinalIgnoreCase) ||
           torrent.RawTitle.Contains(" xx ", StringComparison.OrdinalIgnoreCase)) &&
          !torrent.ParsedTitle.Contains("XXX", StringComparison.OrdinalIgnoreCase));
}
