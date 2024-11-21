namespace Zilean.Scraper.Features.Ingestion.Processing;

public sealed class ProcessedCounts
{
    private int _totalProcessed;
    private int _adultRemoved;
    private int _trashRemoved;
    private int _blacklistedRemoved;

    public void Reset()
    {
        Interlocked.Exchange(ref _totalProcessed, 0);
        Interlocked.Exchange(ref _adultRemoved, 0);
        Interlocked.Exchange(ref _trashRemoved, 0);
        Interlocked.Exchange(ref _blacklistedRemoved, 0);
    }

    public void AddProcessed(int count) => Interlocked.Add(ref _totalProcessed, count);
    public void AddAdultRemoved(int count) => Interlocked.Add(ref _adultRemoved, count);
    public void AddTrashRemoved(int count) => Interlocked.Add(ref _trashRemoved, count);
    public void AddBlacklistedRemoved(int count) => Interlocked.Add(ref _blacklistedRemoved, count);

    public void WriteOutput(ZileanConfiguration configuration, Stopwatch stopwatch, ConcurrentDictionary<string, int>? newPages = null, GenericEndpoint? endpoint = null)
    {
        var table = new Table();

        table.AddColumn("Description");
        table.AddColumn("Count");
        table.AddColumn("Additional Info");

        if (newPages is not null)
        {
            table.AddRow("Processed new DMM pages", newPages.Count.ToString(), $"{newPages.Sum(x => x.Value)} entries");
        }

        if (endpoint is not null)
        {
            table.AddRow("Processed URL", endpoint.Url, $"Type: {endpoint.EndpointType}");
        }

        table.AddRow("Processed torrents", _totalProcessed.ToString(), $"Time Taken: {stopwatch.Elapsed.TotalSeconds:F2}s");

        if (_blacklistedRemoved > 0)
        {
            table.AddRow("Removed Blacklisted Content", _blacklistedRemoved.ToString(), "Due to identification by infohash");
        }

        AnsiConsole.Write(table);
    }
}
