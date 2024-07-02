namespace Zilean.ApiService.Features.Dmm;

public partial class DmmSyncJob(
    ILogger<DmmSyncJob> logger,
    IExamineManager examineManager,
    IDmmFileDownloader dmmFileDownloader,
    DmmSyncState dmmSyncState) : IInvocable,
    ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    [GeneratedRegex("""<iframe src="https:\/\/debridmediamanager.com\/hashlist#(.*)"></iframe>""")]
    private static partial Regex HashCollectionMatcher();

    private readonly string _parsedPageFile = Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");
    private ConcurrentDictionary<string, object> _parsedPages = [];
    private int _processedFilesCount;

    public async Task Invoke()
    {
        dmmSyncState.IsRunning = true;

        await LoadParsedPages();

        var tempDirectory = await dmmFileDownloader.DownloadFileToTempPath(CancellationToken);

        var files = Directory.GetFiles(tempDirectory, "*.html", SearchOption.AllDirectories)
            .Where(f => !_parsedPages.ContainsKey(Path.GetFileName(f)))
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

            _parsedPages.TryAdd(fileName, sanitizedTorrents.Count);
            Interlocked.Increment(ref _processedFilesCount);
        }

        logger.LogInformation("Indexing {Torrents} new torrents", torrents.Count);
        logger.LogInformation("If this is the first run, This process takes a few minutes, please be patient...");

        var resetEvent = new ManualResetEventSlim();

        var valueSets = torrents.Select(torrent => new ValueSet(
            torrent.InfoHash,
            "Torrents",
            new Dictionary<string, object>
            {
                ["Filename"] = torrent.Filename.Replace(".", " ", StringComparison.Ordinal),
                ["Filesize"] = torrent.Filesize,
            }));

        dmmIndexer.IndexOperationComplete += (sender, args) =>
        {
            logger.LogInformation("Indexed {Items} items", args.ItemsIndexed);
            resetEvent.Set();
        };

        dmmIndexer.IndexItems(valueSets);

        resetEvent.Wait(CancellationToken);

        await SaveParsedPages();

        logger.LogInformation("Finished processing {Files} new files", _processedFilesCount);

        dmmSyncState.IsRunning = false;
    }

    private async Task LoadParsedPages()
    {
        if (File.Exists(_parsedPageFile))
        {
            using var reader = new StreamReader(_parsedPageFile);
            _parsedPages = await JsonSerializer.DeserializeAsync<ConcurrentDictionary<string, object>>(reader.BaseStream, cancellationToken: CancellationToken);
        }
    }

    private async Task SaveParsedPages()
    {
        await using var writer = new StreamWriter(_parsedPageFile);
        await JsonSerializer.SerializeAsync(writer.BaseStream, _parsedPages, cancellationToken: CancellationToken);
    }

    private async Task<List<ExtractedDmmEntry>> ExtractPageContents(string filePath, string filenameOnly)
    {
        if (_parsedPages.TryGetValue(filenameOnly, out _) || !File.Exists(filePath))
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
            _parsedPages.TryAdd(filenameOnly, 0);
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
