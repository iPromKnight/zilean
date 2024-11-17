namespace Zilean.Scraper.Features.Imdb;

public class ImdbMetadataLoader(ImdbFileDownloader downloader, ImdbFileProcessor processor, ILogger<ImdbMetadataLoader> logger, ImdbFileService imdbFileService)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        try
        {
            var imdbLastImport = await imdbFileService.GetImdbLastImportAsync(cancellationToken);

            if (imdbLastImport is not null)
            {
                logger.LogInformation("Last import date: {LastImportDate}", imdbLastImport.OccuredAt);
                if (DateTime.UtcNow - imdbLastImport.OccuredAt < TimeSpan.FromDays(14))
                {
                    logger.LogInformation("Imdb Records import is not required as last import was less than 14 days ago");
                    return 0;
                }
            }

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
