namespace Zilean.Shared.Features.Configuration;

public class IngestionConfiguration
{
    public List<GenericEndpoint> ZurgInstances { get; set; } = [];
    public List<GenericEndpoint> ZileanInstances { get; set; } = [];
    public List<GenericEndpoint> GenericInstances { get; set; } = [];
    public bool EnableScraping { get; set; } = false;
    public KubernetesConfiguration Kubernetes { get; set; } = new();
    public string ScrapeSchedule { get; set; } = "0 * * * *";
    public string ZurgEndpointSuffix { get; set; } = "/debug/torrents";
    public string ZileanEndpointSuffix { get; set; } = "/torrents/all";
    public int RequestTimeout { get; set; } = 10000;
}
