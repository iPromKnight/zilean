namespace Zilean.Scraper.Features.Ingestion;

public class KubernetesServiceDiscovery(
    ILogger<KubernetesServiceDiscovery> logger,
    ZileanConfiguration configuration)
{
    private record DiscoveredService(V1Service Service, KubernetesSelector Selector);

    public async Task<List<GenericEndpoint>> DiscoverUrlsAsync(CancellationToken cancellationToken = default)
    {
        var urls = new List<GenericEndpoint>();

        try
        {
            var clientConfig = configuration.Ingestion.Kubernetes.AuthenticationType switch
            {
                KubernetesAuthenticationType.ConfigFile => KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration
                    .Ingestion.Kubernetes.KubeConfigFile),
                KubernetesAuthenticationType.RoleBased => KubernetesClientConfiguration.InClusterConfig(),
                _ => throw new InvalidOperationException("Unknown authentication type")
            };

            var kubernetesClient = new Kubernetes(clientConfig);

            List<DiscoveredService> discoveredServices = [];

            foreach (var selector in configuration.Ingestion.Kubernetes.KubernetesSelectors)
            {
                var services = await kubernetesClient.CoreV1.ListServiceForAllNamespacesAsync(
                    labelSelector: selector.LabelSelector,
                    cancellationToken: cancellationToken);

                discoveredServices.AddRange(services.Items.Select(service => new DiscoveredService(service, selector)));
            }

            foreach (var service in discoveredServices)
            {
                try
                {
                    var url = BuildUrlFromService(service);
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(new GenericEndpoint
                        {
                            EndpointType = service.Selector.EndpointType,
                            Url = url,
                        });
                        logger.LogInformation("Discovered service URL: {Url}", url);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to build URL for service {ServiceName} in namespace {Namespace}",
                        service.Service.Metadata.Name, service.Service.Metadata.NamespaceProperty);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list services with label selectors {@LabelSelector}", configuration.Ingestion.Kubernetes.KubernetesSelectors);
        }

        return urls;
    }

    private string BuildUrlFromService(DiscoveredService service)
    {
        if (service.Service.Metadata?.NamespaceProperty == null)
        {
            throw new InvalidOperationException("Service metadata or namespace is missing.");
        }

        var namespaceName = service.Service.Metadata.NamespaceProperty;
        return string.Format(service.Selector.UrlTemplate, namespaceName);
    }
}
