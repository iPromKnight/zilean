namespace Zilean.ImdbLoader.Features.Imdb;

public class ImdbMetadataLoaderTask
{
    private const int BatchSize = 1_000_000;

    public static async Task<int> Execute(ZileanConfiguration configuration, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<ImdbMetadataLoaderTask>();

        try
        {
            var imdbLoadState = new ImdbLoadState(loggerFactory.CreateLogger<ImdbLoadState>());
            var fileDownloader = new FileDownloader(loggerFactory.CreateLogger<FileDownloader>());
            var elasticClient = new ElasticSearchClient(configuration, loggerFactory.CreateLogger<ElasticSearchClient>());

            await imdbLoadState.SetRunning(cancellationToken);

            var dataFile = await fileDownloader.DownloadMetadataFile(cancellationToken);
            var processor = new FileProcessor(loggerFactory.CreateLogger<FileProcessor>(), elasticClient, imdbLoadState);

            await processor.Import(dataFile, BatchSize, cancellationToken);

            logger.LogInformation("All records processed");

            fileDownloader.DeleteMetadataFile(dataFile);

            await imdbLoadState.SetFinished(cancellationToken);

            logger.LogInformation("ImdbMetadataLoad Internal Tasks Completed");

            return 0;
        }
        catch (TaskCanceledException)
        {
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during ImdbMetadataLoad Task");
            return 1;
        }
    }
}
