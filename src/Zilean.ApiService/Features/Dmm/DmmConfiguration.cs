namespace Zilean.ApiService.Features.Dmm;

public class DmmConfiguration
{
    public bool Enabled { get; set; } = true;
    public string ScrapeSchedule { get; set; } = "0 * * * *";
}
