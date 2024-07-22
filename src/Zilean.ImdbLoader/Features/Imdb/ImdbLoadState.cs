namespace Zilean.ImdbLoader.Features.Imdb;

public class ImdbLoadState(ILogger<ImdbLoadState> logger)
{
    private readonly string _ingestedImdbData = Path.Combine(AppContext.BaseDirectory, "data", "ingestedImdbData.json");
    private IngestedImdbData IngestedData { get; set; } = new();
    private int ProcessedRecordsCount { get; set; }
    public bool IsRunning { get; private set; }

    public void IncrementProcessedRecordsCount(int quantity)
        => ProcessedRecordsCount += quantity;

    public async Task SetRunning(CancellationToken cancellationToken)
    {
        IsRunning = true;
        ProcessedRecordsCount = 0;
        await LoadIngestedImdbDataFile(cancellationToken);
    }

    public async Task SetFinished(CancellationToken cancellationToken)
    {
        logger.LogInformation("Finished processing {Count} imdb files", ProcessedRecordsCount);
        logger.LogInformation("There is a difference of {Difference} records between the ingested and processed data compared to the last run.", ProcessedRecordsCount - IngestedData.RecordCount);
        await SaveParsedPages(cancellationToken);
        IsRunning = false;
    }

    private async Task SaveParsedPages(CancellationToken cancellationToken)
    {
        IngestedData.IngestedAt = DateTime.UtcNow;
        await using var writer = new StreamWriter(_ingestedImdbData);
        await JsonSerializer.SerializeAsync(writer.BaseStream, IngestedData, cancellationToken: cancellationToken);
        logger.LogInformation("Saved {Count} parsed imdb records", IngestedData.RecordCount);
    }

    private async Task LoadIngestedImdbDataFile(CancellationToken cancellationToken)
    {
        if (File.Exists(_ingestedImdbData))
        {
            using var reader = new StreamReader(_ingestedImdbData);
            IngestedData = await JsonSerializer.DeserializeAsync<IngestedImdbData>(reader.BaseStream, cancellationToken: cancellationToken);
            logger.LogInformation("Loaded {Count} previously parsed imdb records", IngestedData.RecordCount);
        }
    }
}
