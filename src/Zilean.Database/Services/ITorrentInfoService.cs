namespace Zilean.Database.Services;

public interface ITorrentInfoService
{
    Task StoreTorrentInfo(IEnumerable<TorrentInfo> torrents);
    Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query);
    Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter);
}
