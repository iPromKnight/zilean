namespace Zilean.ApiService.Features.Dmm;

public partial class DmmSyncJob(
    ILogger<DmmSyncJob> logger,
    IExamineManager examineManager,
    IDmmFileDownloader dmmFileDownloader,
    DmmSyncState dmmState) : IInvocable,
    ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    [GeneratedRegex("""<iframe src="https:\/\/debridmediamanager.com\/hashlist#(.*)"></iframe>""")]
    private static partial Regex HashCollectionMatcher();

    public async Task Invoke()
    {
        if (!dmmState.IsRunning)
        {
            await dmmState.SetRunning(CancellationToken);
        }

        var tempDirectory = await dmmFileDownloader.DownloadFileToTempPath(CancellationToken);

        var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
            .Where(f => !dmmState.ParsedPages.ContainsKey(Path.GetFileName(f)))
            .ToArray();

        logger.LogInformation("Found {Files} files to parse", files.Length);

        if (!examineManager.TryGetIndex("DMM", out var dmmIndexer))
        {
            logger.LogError("Failed to get dmm lucene indexer, aborting...");
            return;
        }

        var torrents = new List<ExtractedDmmEntry>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var sanitizedTorrents = await ExtractPageContents(file, fileName);

            torrents.AddRange(sanitizedTorrents);

            dmmState.ParsedPages.TryAdd(fileName, sanitizedTorrents.Count);
            dmmState.IncrementProcessedFilesCount();
        }

        var distinctTorrents = torrents.DistinctBy(x=>x.InfoHash).ToList();

        logger.LogInformation("Total torrents from files: {Torrents}", torrents.Count);
        logger.LogInformation("Indexing {Torrents} distinct new torrents", distinctTorrents.Count);
        logger.LogInformation("If this is the first run, This process takes a few minutes, please be patient...");

        var valueSets = distinctTorrents.DistinctBy(x=>x.InfoHash).Select(torrent => new ValueSet(
            torrent.InfoHash,
            "Torrents",
            new Dictionary<string, object>
            {
                ["Filename"] = torrent.Filename.Replace(".", " ", StringComparison.Ordinal),
                ["Filesize"] = torrent.Filesize,
            }));

        SetupEventHandlerForIndexer(dmmIndexer, dmmState.SyncProcessResetEvent);

        dmmIndexer.IndexItems(valueSets);

        dmmState.SyncProcessResetEvent.Wait(CancellationToken);

        await dmmState.SetFinished(CancellationToken);
    }

    private void SetupEventHandlerForIndexer(IIndex dmmIndexer, ManualResetEventSlim resetEvent)
    {
        dmmIndexer.IndexOperationComplete -= IndexingOperationCompleteCallback(resetEvent);
        dmmIndexer.IndexOperationComplete += IndexingOperationCompleteCallback(resetEvent);
    }

    private EventHandler<IndexOperationEventArgs> IndexingOperationCompleteCallback(ManualResetEventSlim resetEvent) =>
        (_, args) =>
        {
            logger.LogInformation("Indexed {Items} items", args.ItemsIndexed);
            resetEvent.Set();
        };

    private async Task<List<ExtractedDmmEntry>> ExtractPageContents(string filePath, string filenameOnly)
    {
        if (dmmState.ParsedPages.TryGetValue(filenameOnly, out _) || !File.Exists(filePath))
        {
            return [];
        }

        var pageSource = await File.ReadAllTextAsync(filePath, CancellationToken);

        var match = HashCollectionMatcher().Match(pageSource);

        if (!match.Success)
        {
            return [];
        }

        var encodedJson = match.Groups.Values.ElementAtOrDefault(1);

        if (string.IsNullOrEmpty(encodedJson?.Value))
        {
            logger.LogWarning("Failed to extract encoded json for {Name}", filenameOnly);
            return [];
        }

        var decodedJson = LZString.DecompressFromEncodedURIComponent(encodedJson.Value);

        using var json = JsonDocument.Parse(decodedJson);

        var torrents = await json.RootElement.EnumerateArray()
            .ToAsyncEnumerable()
            .Select(ParsePageContent)
            .Where(t => t is not null)
            .ToListAsync(cancellationToken: CancellationToken);

        if (torrents.Count == 0)
        {
            logger.LogWarning("No torrents found in {Name}", filenameOnly);
            dmmState.ParsedPages.TryAdd(filenameOnly, 0);
            return [];
        }

        var sanitizedTorrents = torrents
            .Where(x => x is not null)
            .GroupBy(x => x.InfoHash)
            .Select(g => new ExtractedDmmEntry(g.First().Filename, g.Key, g.First().Filesize))
            .ToList();

        logger.LogInformation("Parsed {Torrents} torrents for {Name}", sanitizedTorrents.Count, filenameOnly);

        return sanitizedTorrents;
    }

    private static ExtractedDmmEntry? ParsePageContent(JsonElement item) =>
        !item.TryGetProperty("filename", out var filenameElement) ||
        !item.TryGetProperty("bytes", out var filesizeElement) ||
        !item.TryGetProperty("hash", out var hashElement)
            ? null
            : new ExtractedDmmEntry(filenameElement.GetString(), hashElement.GetString(), filesizeElement.GetInt64());
}
