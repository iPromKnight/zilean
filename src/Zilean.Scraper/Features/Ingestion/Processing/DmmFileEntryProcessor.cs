namespace Zilean.Scraper.Features.Ingestion.Processing;

public partial class DmmFileEntryProcessor(
    DmmService dmmService,
    ITorrentInfoService torrentInfoService,
    ParseTorrentNameService parseTorrentNameService,
    ILoggerFactory loggerFactory,
    ZileanConfiguration configuration) : GenericProcessor<ExtractedDmmEntry>(loggerFactory, torrentInfoService, parseTorrentNameService, configuration)
{
    [GeneratedRegex("""<iframe src="https:\/\/debridmediamanager.com\/hashlist#(.*)"></iframe>""")]
    private static partial Regex HashCollectionMatcher { get; }
    private List<string> _filesToProcess = [];
    private readonly ObjectPool<List<ExtractedDmmEntry>> _torrentsListPool = new DefaultObjectPoolProvider().Create<List<ExtractedDmmEntry>>();
    public ConcurrentDictionary<string, int> ExistingPages { get; private set; } = [];
    public ConcurrentDictionary<string, int> NewPages { get; set; } = [];
    protected override ExtractedDmmEntry TransformToTorrent(ExtractedDmmEntry input) => input;

    public async Task ProcessFilesAsync(List<string> files, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _filesToProcess = files;
        _processedCounts.Reset();
        _logger.LogInformation("Processing {Count} DMM Files", _filesToProcess.Count);
        await ProcessAsync(ProduceEntriesAsync, cancellationToken);
        _processedCounts.WriteOutput(_configuration, sw, NewPages);
        sw.Stop();
    }

    private async Task ProduceEntriesAsync(ChannelWriter<Task<ExtractedDmmEntry>> writer, CancellationToken cancellationToken)
    {
        foreach (var file in _filesToProcess)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Processing canceled.");
                break;
            }

            var fileName = Path.GetFileName(file);
            if (ExistingPages.TryGetValue(fileName, out _) || NewPages.TryGetValue(fileName, out _))
            {
                continue;
            }

            _logger.LogInformation("Processing file: {FileName}", fileName);

            try
            {
                var torrents = await ProcessPageAsync(file, fileName, cancellationToken);
                foreach (var torrent in torrents)
                {
                    await writer.WriteAsync(Task.FromResult(torrent), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", fileName);
            }
        }

        writer.Complete();
    }

    private async Task<List<ExtractedDmmEntry>> ProcessPageAsync(string filePath, string filenameOnly, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var pageSource = await File.ReadAllTextAsync(filePath, cancellationToken);
        var match = HashCollectionMatcher.Match(pageSource);

        if (!match.Success)
        {
            await AddParsedPage(filenameOnly, 0, cancellationToken);
            return [];
        }

        var torrents = _torrentsListPool.Get();

        try
        {
            var decodedJson = Decompressor.FromEncodedUriComponent(match.Groups[1].Value);

            var utf8Bytes = Encoding.UTF8.GetBytes(decodedJson);
            var span = new ReadOnlySpan<byte>(utf8Bytes);
            var reader = new Utf8JsonReader(span);

            ParseTorrents(ref reader, torrents);

            if (torrents.Count == 0)
            {
                await AddParsedPage(filenameOnly, 0, cancellationToken);
                return [];
            }

            var sanitizedTorrents = torrents
                .Where(x => x.Filesize > 0)
                .GroupBy(x => x.InfoHash)
                .Select(group => group.FirstOrDefault())
                .Where(x => !string.IsNullOrEmpty(x.Filename))
                .OfType<ExtractedDmmEntry>()
                .ToList();

            await AddParsedPage(filenameOnly, sanitizedTorrents.Count, cancellationToken);
            return sanitizedTorrents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing file: {FilePath}", filePath);
            await AddParsedPage(filenameOnly, 0, cancellationToken);
            return [];
        }
        finally
        {
            torrents.Clear();
            _torrentsListPool.Return(torrents);
        }
    }

    private static ExtractedDmmEntry? ParsePageContent(ref Utf8JsonReader reader)
    {
        string? filename = null;
        long filesize = 0;
        string? hash = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "filename":
                        filename = reader.GetString();
                        break;
                    case "bytes":
                        filesize = reader.GetInt64();
                        break;
                    case "hash":
                        hash = reader.GetString();
                        break;
                }
            }
        }

        return filename == null || hash == null
            ? null
            : new ExtractedDmmEntry(hash, filename.Replace(".", " ", StringComparison.Ordinal), filesize, null);
    }

    public async Task LoadParsedPages(CancellationToken cancellationToken)
    {
        var parsedPages = await dmmService.GetIngestedPagesAsync(cancellationToken);
        if (parsedPages.Count != 0)
        {
            ExistingPages = new ConcurrentDictionary<string, int>(parsedPages.ToDictionary(x => x.Page, x => x.EntryCount));
        }

        _logger.LogInformation("Loaded {Count} previously parsed pages", ExistingPages.Count);
    }

    private async Task AddParsedPage(string filename, int entryCount, CancellationToken cancellationToken)
    {
        await dmmService.AddPageToIngestedAsync(new ParsedPages
        {
            EntryCount = entryCount,
            Page = filename
        }, cancellationToken);

        NewPages.TryAdd(filename, entryCount);
    }

    [SuppressMessage("Style", "IDE0010:Add missing cases")]
    private static void ParseTorrents(ref Utf8JsonReader reader, List<ExtractedDmmEntry> torrents)
    {
        if (!reader.Read())
        {
            return;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                ParseArray(ref reader, torrents);
                return;
            case JsonTokenType.StartObject:
                ParseNestedTorrents(ref reader, torrents);
                break;
        }
    }

    private static void ParseNestedTorrents(ref Utf8JsonReader reader, List<ExtractedDmmEntry> torrents)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "torrents")
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    ParseArray(ref reader, torrents);
                }
                break;
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
        }
    }

    private static void ParseArray(ref Utf8JsonReader reader, List<ExtractedDmmEntry> torrents)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var entry = ParsePageContent(ref reader);
                if (entry != null)
                {
                    torrents.Add(entry);
                }
            }
            else if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }
        }
    }
}
