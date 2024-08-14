namespace Zilean.Database.Dtos;

public class ImdbSearchResult
{
    public string? Title { get; set; }
    public string? ImdbId { get; set; }
    public int Year { get; set; }
    public double Score { get; set; }
    public string? Category { get; set; }
}
