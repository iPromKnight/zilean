namespace Zilean.DmmScraper.Features.Dmm;

public partial class DmmPageProcessor(DmmSyncState state)
    : IDisposable
{
    [GeneratedRegex("""<iframe src="https:\/\/debridmediamanager.com\/hashlist#(.*)"></iframe>""")]
    private static partial Regex HashCollectionMatcher();

    public async Task<List<ExtractedDmmEntry>> ProcessPageAsync(string filePath, string filenameOnly, CancellationToken cancellationToken)
    {
        if (state.ParsedPages.TryGetValue(filenameOnly, out _) || !File.Exists(filePath))
        {
            return [];
        }

        var pageSource = await File.ReadAllTextAsync(filePath, cancellationToken);

        var match = HashCollectionMatcher().Match(pageSource);

        if (!match.Success)
        {
            return [];
        }

        try
        {
            var decodedJson = Decompressor.FromEncodedUriComponent(match.Groups[1].Value);
            var byteArray = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(decodedJson));

            try
            {
                var byteCount = Encoding.UTF8.GetBytes(decodedJson, byteArray);
                using var memoryStream = new MemoryStream(byteArray, 0, byteCount);
                using var json = await JsonDocument.ParseAsync(memoryStream, cancellationToken: cancellationToken);

                var torrents = json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("torrents", out var torrentsElement)
                    ? torrentsElement.EnumerateArray().Select(ParsePageContent).OfType<ExtractedDmmEntry>().ToList()
                    : json.RootElement.EnumerateArray().Select(ParsePageContent).OfType<ExtractedDmmEntry>().ToList();

                if (torrents.Count == 0)
                {
                    state.ParsedPages.TryAdd(filenameOnly, 0);
                    return [];
                }

                var sanitizedTorrents = torrents
                    .Where(x=> x.Filesize > 0)
                    .GroupBy(x => x.InfoHash)
                    .Select(group => group.FirstOrDefault())
                    .Where(x => !string.IsNullOrEmpty(x.Filename))
                    .OfType<ExtractedDmmEntry>()
                    .ToList();

                return sanitizedTorrents;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(byteArray);
            }
        }
        catch
        {
            state.ParsedPages.TryAdd(filenameOnly, 0);
            return [];
        }
    }

    private static ExtractedDmmEntry? ParsePageContent(JsonElement item)
    {
        var filename = item.TryGetProperty("filename", out var filenameElement);
        var filesize = item.TryGetProperty("bytes", out var filesizeElement);
        var hash = item.TryGetProperty("hash", out var hashElement);

        if (!filename || !filesize || !hash)
        {
            return null;
        }

        var fileText = filenameElement.GetString();
        var fileSize = filesizeElement.GetInt64();
        var hashText = hashElement.GetString();

        var entry = new ExtractedDmmEntry(hashText, fileText.Replace(".", " ", StringComparison.Ordinal), fileSize, null);

        return entry;
    }

    public void Dispose()
    {
        GC.Collect();
        GC.SuppressFinalize(this);
    }
}
