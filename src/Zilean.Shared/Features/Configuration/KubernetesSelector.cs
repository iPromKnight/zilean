namespace Zilean.Shared.Features.Configuration;

public class KubernetesSelector
{
    public string UrlTemplate { get; set; } = "http://zurg.{0}:9999/debug/torrents";
    public string LabelSelector { get; set; } = "app.elfhosted.com/name=zurg";
    public GenericEndpointType EndpointType { get; set; } = GenericEndpointType.Zurg;
}
