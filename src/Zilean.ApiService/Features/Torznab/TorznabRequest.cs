// ReSharper disable InconsistentNaming
namespace Zilean.ApiService.Features.Torznab;

public class TorznabRequest
{
    public string? q { get; set; }
    public string? imdbid { get; set; }
    public string? ep { get; set; }
    public string? t { get; set; }
    public string? extended { get; set; }
    public string? limit { get; set; }
    public string? offset { get; set; }
    public string? cat { get; set; }
    public string? season { get; set; }
    public string? year { get; set; }
}
