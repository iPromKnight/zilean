﻿namespace Zilean.Shared.Features.Configuration;

public class KubernetesConfiguration
{
    public bool EnableServiceDiscovery { get; set; } = false;
    public List<KubernetesSelector> KubernetesSelectors { get; set; } = [new()];
    public string KubeConfigFile { get; set; } = "/$HOME/.kube/config";
    public bool IsConfigFile { get; set; } = false;
}
