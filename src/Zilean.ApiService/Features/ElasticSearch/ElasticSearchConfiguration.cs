namespace Zilean.ApiService.Features.ElasticSearch;

public class ElasticSearchConfiguration
{
    public string Url { get; set; } = "http://localhost:9200";
    public string ApiKey { get; set; } = string.Empty;
}
