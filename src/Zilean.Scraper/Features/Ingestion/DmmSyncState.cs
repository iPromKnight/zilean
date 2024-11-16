namespace Zilean.Scraper.Features.Ingestion;

public class DmmSyncState(ILogger<DmmSyncState> logger, DmmService dmmService)
{
    private int ProcessedFilesCount { get; set; }
    public bool IsRunning { get; private set; }
    public ConcurrentDictionary<string, int> ParsedPages { get; set; } = [];
    public ConcurrentDictionary<string, int> ExistingPages { get; set; } = [];

    public void IncrementProcessedFilesCount() => ProcessedFilesCount++;

    public async Task SetRunning(CancellationToken cancellationToken)
    {
        IsRunning = true;
        ParsedPages = [];
        ExistingPages = [];
        ProcessedFilesCount = 0;
        await LoadParsedPages(cancellationToken);
    }

    public async Task SetFinished(CancellationToken cancellationToken, DmmPageProcessor processor)
    {
        logger.LogInformation("Finished processing {Count} new files", ProcessedFilesCount);
        await SaveParsedPages(cancellationToken);
        IsRunning = false;
        ParsedPages = [];
        ExistingPages = [];
        ProcessedFilesCount = 0;
        processor.Dispose();
    }

    private async Task LoadParsedPages(CancellationToken cancellationToken)
    {
        var parsedPages = await dmmService.GetIngestedPagesAsync(cancellationToken);
        if (parsedPages.Any())
        {
            ExistingPages= new ConcurrentDictionary<string, int>(parsedPages.ToDictionary(x => x.Page, x => x.EntryCount));
        }

        logger.LogInformation("Loaded {Count} previously parsed pages", ExistingPages.Count);
    }

    private async Task SaveParsedPages(CancellationToken cancellationToken)
    {
        if (ParsedPages.IsEmpty)
        {
            logger.LogInformation("No parsed pages to store.");
            return;
        }

        var pages = ParsedPages.Select(x => new ParsedPages { Page = x.Key, EntryCount = x.Value }).ToList();

        await dmmService.AddPagesToIngestedAsync(pages, cancellationToken);
        logger.LogInformation("Stored {Count} parsed pages", ParsedPages.Count);
    }
}
