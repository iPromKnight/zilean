namespace Zilean.ApiService.Features.Dmm;

public class DmmSyncState(ILogger<DmmSyncState> logger)
{
    private readonly string _parsedPageFile = Path.Combine(AppContext.BaseDirectory, "data", "parsedPages.json");
    private int ProcessedFilesCount { get; set; }
    public bool IsRunning { get; private set; }
    public ConcurrentDictionary<string, object> ParsedPages { get; set; } = [];

    public void IncrementProcessedFilesCount() => ProcessedFilesCount++;

    public async Task SetRunning(CancellationToken cancellationToken)
    {
        IsRunning = true;
        ParsedPages = new ConcurrentDictionary<string, object>();
        ProcessedFilesCount = 0;
        await LoadParsedPages(cancellationToken);
    }

    public async Task SetFinished(CancellationToken cancellationToken)
    {
        logger.LogInformation("Finished processing {Files} new files", ProcessedFilesCount);
        await SaveParsedPages(cancellationToken);
        IsRunning = false;
        ParsedPages = new ConcurrentDictionary<string, object>();
        ProcessedFilesCount = 0;
    }

    private async Task LoadParsedPages(CancellationToken cancellationToken)
    {
        if (File.Exists(_parsedPageFile))
        {
            using var reader = new StreamReader(_parsedPageFile);
            ParsedPages = await JsonSerializer.DeserializeAsync<ConcurrentDictionary<string, object>>(reader.BaseStream, cancellationToken: cancellationToken);
            logger.LogInformation("Loaded {Files} parsed pages", ParsedPages.Count);
        }
    }

    private async Task SaveParsedPages(CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(_parsedPageFile);
        await JsonSerializer.SerializeAsync(writer.BaseStream, ParsedPages, cancellationToken: cancellationToken);
        logger.LogInformation("Saved {Files} parsed pages", ParsedPages.Count);
    }
}
