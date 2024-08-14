namespace Zilean.ApiService.Features.Imdb;

public class ImdbFilteredRequest
{
    public string? Query { get; init; }
    public int? Year { get; init; }
    public string? Category { get; init; }
}
