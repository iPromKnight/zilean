namespace Zilean.Database.Services;

public interface ITorrentInfoService
{
    Task StoreTorrentInfo(List<TorrentInfo> torrents, int batchSize = 10000);
    Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query);
    Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter, int? limit = null);
    Task<HashSet<string>> GetExistingInfoHashesAsync(List<string> infoHashes);
    Task<HashSet<string>> GetBlacklistedItems();
    Task VaccumTorrentsIndexes(CancellationToken cancellationToken);

    Task<ConcurrentBag<TorrentInfo>> MatchImdbIdsForBatchAsync(
        IEnumerable<TorrentInfo> batch,
        ConcurrentDictionary<int, List<ImdbFile>> imdbTvFilesByYear,
        ConcurrentDictionary<int, List<ImdbFile>> imdbMovieFilesByYear);
}
