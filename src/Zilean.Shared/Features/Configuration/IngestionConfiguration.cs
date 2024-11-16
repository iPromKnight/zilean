namespace Zilean.Shared.Features.Configuration;

public class IngestionConfiguration
{
    public List<string> ZurgInstances { get; set; } = [];
    public List<string> ZileanInstances { get; set; } = [];
    public bool EnableZurgIngestion { get; set; } = false;
    public bool EnableZileanIngestion { get; set; } = false;
    public KubernetesConfiguration Kubernetes { get; set; } = new();
    public int BatchSize { get; set; } = 1000;
    public int MaxChannelSize { get; set; } = 5000;
}
