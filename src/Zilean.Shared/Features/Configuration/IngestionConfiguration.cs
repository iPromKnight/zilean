namespace Zilean.Shared.Features.Configuration;

public class IngestionConfiguration
{
    public List<string> ZurgInstances { get; set; } = [];
    public List<string> ZileanInstances { get; set; } = [];
    public bool EnableScraping { get; set; } = false;
    public KubernetesConfiguration Kubernetes { get; set; } = new();
    public int BatchSize { get; set; } = 500;
    public int MaxChannelSize { get; set; } = 5000;
    public string ScrapeSchedule { get; set; } = "0 * * * *";
    public string ZurgEndpointSuffix { get; set; } = "/debug/torrents";
    public string ZileanEndpointSuffix { get; set; } = "/torrents/all";
}
