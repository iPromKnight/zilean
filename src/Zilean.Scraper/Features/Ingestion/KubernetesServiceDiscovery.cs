namespace Zilean.Scraper.Features.Ingestion;

public class KubernetesServiceDiscovery(
    ILogger<KubernetesServiceDiscovery> logger,
    ZileanConfiguration configuration)
{
    public async Task<List<string>> DiscoverUrlsAsync(CancellationToken cancellationToken = default)
    {
        var urls = new List<string>();

        try
        {
            var clientConfig =
                KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration.Ingestion.Kubernetes.KubeConfigFile);
            var kubernetesClient = new Kubernetes(clientConfig);

            var services = await kubernetesClient.CoreV1.ListServiceForAllNamespacesAsync(
                labelSelector: configuration.Ingestion.Kubernetes.LabelSelector,
                cancellationToken: cancellationToken);

            foreach (var service in services.Items)
            {
                try
                {
                    var url = BuildUrlFromService(service);
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(url);
                        logger.LogInformation("Discovered service URL: {Url}", url);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to build URL for service {ServiceName} in namespace {Namespace}",
                        service.Metadata.Name, service.Metadata.NamespaceProperty);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list services with label selector {LabelSelector}", configuration.Ingestion.Kubernetes.LabelSelector);
        }

        return urls;
    }

    private string BuildUrlFromService(V1Service service)
    {
        if (service.Metadata?.NamespaceProperty == null)
        {
            throw new InvalidOperationException("Service metadata or namespace is missing.");
        }

        var namespaceName = service.Metadata.NamespaceProperty;
        return string.Format(configuration.Ingestion.Kubernetes.ZurgUrlTemplate, namespaceName);
    }
}
