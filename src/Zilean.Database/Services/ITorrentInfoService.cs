namespace Zilean.Database.Services;

public interface ITorrentInfoService
{
    Task StoreTorrentInfo(IEnumerable<TorrentInfo> torrents);
    Task<ExtractedDmmEntryResponse[]> SearchForTorrentInfoByOnlyTitle(string query);
    Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter);
}
