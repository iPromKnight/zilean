namespace Zilean.ImdbLoader.Features.Imdb;

public class FileProcessor(
    ILogger<FileProcessor> logger,
    ElasticSearchClient elasticClient,
    ImdbLoadState imdbLoadState)
{
    public async Task Import(string fileName, int batchSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing Downloaded IMDB Basics data from {FilePath}", fileName);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            BadDataFound = null, // Skip Bad Data from Imdb
            MissingFieldFound = null, // Skip Missing Fields from Imdb
        };

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, csvConfig);

        var channel = Channel.CreateBounded<ImdbFile>(new BoundedChannelOptions(batchSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });

        await csv.ReadAsync();

        var batchInsertTask = CreateBatchOfBasicEntries(channel, batchSize, cancellationToken);

        await ReadBasicEntries(csv, channel, cancellationToken);

        channel.Writer.Complete();

        await batchInsertTask;
    }

    private Task CreateBatchOfBasicEntries(Channel<ImdbFile> channel, int batchSize, CancellationToken cancellationToken) =>
        Task.Run(async () =>
        {
            var buffer = new List<ImdbFile>();

            await foreach (var movieData in channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                buffer.Add(movieData);

                if (buffer.Count >= batchSize)
                {
                    await ProcessBatch(buffer, cancellationToken);
                    buffer.Clear();
                }
            }

            // Process any remaining items in the buffer
            if (buffer.Count > 0)
            {
                await ProcessBatch(buffer, cancellationToken);
            }
        }, cancellationToken);

    private async Task ProcessBatch(List<ImdbFile> batch, CancellationToken cancellationToken)
    {
        if (batch.Count > 0)
        {
            await elasticClient.IndexManyBatchedAsync(batch, ElasticSearchClient.ImdbMetadataIndex, cancellationToken);
            imdbLoadState.IncrementProcessedRecordsCount(batch.Count);
            logger.LogInformation("Imported batch of {BatchSize} basics starting with ImdbId {FirstImdbId}", batch.Count,
                batch.First().ImdbId);
        }
    }

    private static async Task ReadBasicEntries(CsvReader csv, Channel<ImdbFile> channel, CancellationToken cancellationToken)
    {
        while (await csv.ReadAsync())
        {
            var isAdultSet = int.TryParse(csv.GetField(4), out var adult);

            var movieData = new ImdbFile
            {
                ImdbId = csv.GetField(0),
                Category = csv.GetField(1),
                Title = csv.GetField(2),
                Adult = isAdultSet && adult == 1,
                Year = csv.GetField(5) == @"\N" ? 0 : int.Parse(csv.GetField(5)),
            };

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await channel.Writer.WriteAsync(movieData, cancellationToken);
        }
    }
}
