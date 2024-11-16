using Zilean.Scraper.Features.LzString;

namespace Zilean.Scraper.Features.Dmm;

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
                var span = new ReadOnlySpan<byte>(byteArray, 0, byteCount);
                var reader = new Utf8JsonReader(span);

                var torrents = new List<ExtractedDmmEntry>();

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
                }

                if (torrents.Count == 0)
                {
                    state.ParsedPages.TryAdd(filenameOnly, 0);
                    return [];
                }

                var sanitizedTorrents = torrents
                    .Where(x => x.Filesize > 0)
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

        if (filename == null || hash == null)
        {
            return null;
        }

        var entry = new ExtractedDmmEntry(hash, filename.Replace(".", " ", StringComparison.Ordinal), filesize, null);
        return entry;
    }

    public void Dispose()
    {
        GC.Collect();
        GC.SuppressFinalize(this);
    }
}
