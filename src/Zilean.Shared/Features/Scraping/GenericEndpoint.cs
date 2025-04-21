namespace Zilean.Shared.Features.Scraping;

public class GenericEndpoint
{
    public string? Url { get; set; }
    public GenericEndpointType? EndpointType { get; set; }
    public string? ApiKey { get; set; }
    public string? Authorization { get; set; }
    public string? EndpointSuffix { get; set; }
}
