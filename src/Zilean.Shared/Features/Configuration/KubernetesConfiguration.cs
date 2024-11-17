namespace Zilean.Shared.Features.Configuration;

public class KubernetesConfiguration
{
    public bool EnableServiceDiscovery { get; set; } = false;
    public List<KubernetesSelector> KubernetesSelectors { get; set; } = [];
    public string KubeConfigFile { get; set; } = "/$HOME/.kube/config";

    public KubernetesAuthenticationType AuthenticationType { get; set; } = KubernetesAuthenticationType.ConfigFile;
}
