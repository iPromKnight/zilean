namespace Zilean.Shared.Features.Configuration;

public class KubernetesConfiguration
{
    public bool EnableServiceDiscovery { get; set; } = false;
    public string ZurgUrlTemplate { get; set; } = "http://zurg.{0}:9999/debug/torrents";
    public string LabelSelector { get; set; } = "app.elfhosted.com/name=zurg";
    public string KubeConfigFile { get; set; } = "/$HOME/.kube/config";
    public bool IsConfigFile { get; set; } = false;
}
