namespace Zilean.Shared.Features.Scraping;

public class GenericEndpoint
{
    public required string Url { get; set; }
    public required GenericEndpointType EndpointType { get; set; }
}
