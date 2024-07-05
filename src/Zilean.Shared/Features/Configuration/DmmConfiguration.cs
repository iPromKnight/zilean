namespace Zilean.Shared.Features.Configuration;

public class DmmConfiguration
{
    public bool EnableScraping { get; set; } = true;
    public bool EnableEndpoint { get; set; } = true;
    public string ScrapeSchedule { get; set; } = "0 * * * *";
}
