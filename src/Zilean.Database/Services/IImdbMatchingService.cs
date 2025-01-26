namespace Zilean.Database.Services;

public interface IImdbMatchingService
{
    Task<ConcurrentQueue<TorrentInfo>> MatchImdbIdsForBatchAsync(IEnumerable<TorrentInfo> batch);
    Task PopulateImdbData();
    void DisposeImdbData();
}
