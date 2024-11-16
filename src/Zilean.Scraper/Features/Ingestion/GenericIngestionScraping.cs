namespace Zilean.Scraper.Features.Ingestion;

public class GenericIngestionScraping(
    ZileanConfiguration configuration,
    GenericIngestionProcessor ingestionProcessor,
    ILogger<GenericIngestionScraping> logger,
    KubernetesServiceDiscovery kubernetesServiceDiscovery)
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting ingestion scraping");

        List<GenericEndpoint> urlsToProcess = [];

        configuration.Ingestion.ZileanInstances.Add("http://localhost:8181");
        configuration.Ingestion.ZurgInstances.Add("http://experiments:19999");

        await DiscoverUrlsFromKubernetesServices(cancellationToken, urlsToProcess);

        AddZurgInstancesToUrls(urlsToProcess);

        AddZileanInstancesToUrls(urlsToProcess);

        if (urlsToProcess.Count == 0)
        {
            logger.LogInformation("No URLs to process, exiting");
            return 0;
        }

        var completedCount = 0;

        foreach (var url in urlsToProcess)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ingestionProcessor.ProcessTorrentsAsync(url, cancellationToken);
                completedCount++;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Ingestion scraping cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing URL: {@Url}", url);
            }
        }

        logger.LogInformation("Ingestion scraping completed for {Count} URLs", completedCount);

        return 0;
    }

    private void AddZileanInstancesToUrls(List<GenericEndpoint> urlsToProcess)
    {
        if (configuration.Ingestion.ZileanInstances.Count > 0)
        {
            logger.LogInformation("Adding Zilean instances to the list of URLs to process");
            urlsToProcess.AddRange(configuration.Ingestion.ZileanInstances.Select(url => new GenericEndpoint
            {
                EndpointType = GenericEndpointType.Zilean,
                Url = url,
            }));
        }
    }

    private void AddZurgInstancesToUrls(List<GenericEndpoint> urlsToProcess)
    {
        if (configuration.Ingestion.ZurgInstances.Count > 0)
        {
            logger.LogInformation("Adding Zurg instances to the list of URLs to process");
            urlsToProcess.AddRange(configuration.Ingestion.ZurgInstances.Select(url => new GenericEndpoint
            {
                EndpointType = GenericEndpointType.Zurg,
                Url = url,
            }));
        }
    }

    private async Task DiscoverUrlsFromKubernetesServices(CancellationToken cancellationToken, List<GenericEndpoint> urlsToProcess)
    {
        if (configuration.Ingestion.Kubernetes.EnableServiceDiscovery)
        {
            logger.LogInformation("Discovering URLs from Kubernetes services");
            var endpoints = await kubernetesServiceDiscovery.DiscoverUrlsAsync(cancellationToken);
            logger.LogInformation("Discovered {Count} URLs from Kubernetes services", endpoints.Count);
            urlsToProcess.AddRange(endpoints);
        }
    }
}
