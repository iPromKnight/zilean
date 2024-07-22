namespace Zilean.Shared.Features.Configuration;

public class ImdbConfiguration
{
    public bool EnableScraping { get; set; } = true;
    public bool EnableEndpoint { get; set; } = true;
    public string ScrapeSchedule { get; set; } = "0 0 1 */2 *";

    public int MinimumScoreMatch { get; set; } = 85;
}
