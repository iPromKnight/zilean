namespace Zilean.Shared.Features.Configuration;

public class TorrentsConfiguration
{
    public bool EnableEndpoint { get; set; } = false;
    public int MaxHashesToCheck { get; set; } = 100;
}
