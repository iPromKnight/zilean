namespace Zilean.Shared.Features.Configuration;

public class ImdbConfiguration
{
    public bool EnableImportMatching { get; set; } = true;
    public bool EnableEndpoint { get; set; } = true;
    public double MinimumScoreMatch { get; set; } = 0.85;
}
