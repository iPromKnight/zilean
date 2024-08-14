namespace Zilean.Shared.Features.Configuration;

public class ImdbConfiguration
{
    public bool EnableScraping { get; set; } = true;
    public bool EnableEndpoint { get; set; } = true;
    public int MinimumScoreMatch { get; set; } = 90;
}
