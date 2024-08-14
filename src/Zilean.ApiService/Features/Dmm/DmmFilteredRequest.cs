namespace Zilean.ApiService.Features.Dmm;

public class DmmFilteredRequest
{
    public string? Query { get; init; }
    public int? Season { get; init; }
    public int? Episode { get; init; }
    public int? Year { get; init; }
    public string? Language { get; init; }
    public string? Resolution { get; init; }
    public string? ImdbId { get; init; }
}
