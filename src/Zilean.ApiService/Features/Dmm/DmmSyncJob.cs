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

    private readonly int _parallelismCount = 2;
    private readonly string _parsedPageFile = Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");
    private ConcurrentDictionary<string, object> _parsedPages = [];
    private readonly int _saveInterval = 50;
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

        var options = new ParallelOptions { MaxDegreeOfParallelism = _parallelismCount };

        if (!examineManager.TryGetIndex("DMM", out var dmmIndexer))
        {
            logger.LogError("Failed to get dmm lucene indexer, aborting...");
            return;
        }

        await Parallel.ForEachAsync(files, options, async (file, cToken) =>
        {
            if (cToken.IsCancellationRequested)
            {
                return;
            }

            var fileName = Path.GetFileName(file);
            var sanitizedTorrents = await ExtractPageContents(file, fileName);

            if (sanitizedTorrents.Count == 0)
            {
                return;
            }

            var valueSets = sanitizedTorrents.Select(torrent => new ValueSet(
                torrent.InfoHash,
                "Torrents",
                new Dictionary<string, object>
                {
                    ["Filename"] = torrent.Filename.Replace(".", " ", StringComparison.Ordinal),
                    ["Filesize"] = torrent.Filesize,
                }));

            dmmIndexer.IndexItems(valueSets);

            _parsedPages.TryAdd(fileName, sanitizedTorrents.Count);
            Interlocked.Increment(ref _processedFilesCount);

            if (_processedFilesCount % _saveInterval == 0)
            {
                await SaveParsedPages();
            }
        });

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

        var json = JsonDocument.Parse(decodedJson);

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
