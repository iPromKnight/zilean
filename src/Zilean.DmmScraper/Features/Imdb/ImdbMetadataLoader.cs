namespace Zilean.DmmScraper.Features.Imdb;

public class ImdbMetadataLoader(ImdbFileDownloader downloader, ImdbFileProcessor processor, ILogger<ImdbMetadataLoader> logger)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        try
        {
            var dataFile = await downloader.DownloadMetadataFile(cancellationToken);

            await processor.Import(dataFile, cancellationToken);

            logger.LogInformation("All IMDB records processed");

            logger.LogInformation("ImdbMetadataLoader Tasks Completed");

            return 0;
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("ImdbMetadataLoader Task Cancelled");
            return 1;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("ImdbMetadataLoader Task Cancelled");
            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during ImdbMetadataLoader Task");
            return 1;
        }
    }
}
