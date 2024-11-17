namespace Zilean.Database.Services;

public interface ITorrentInfoService
{
    Task StoreTorrentInfo(List<TorrentInfo> torrents, int batchSize = 10000);
    Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query);
    Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter, int? limit = null);
    Task<HashSet<string>> GetExistingInfoHashesAsync(List<string> infoHashes);
    Task<HashSet<string>> GetBlacklistedItems();
}
