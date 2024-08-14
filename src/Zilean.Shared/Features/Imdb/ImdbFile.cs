using System.ComponentModel.DataAnnotations;

namespace Zilean.Shared.Features.Imdb;

public class ImdbFile
{
    [Key]
    public string ImdbId { get; set; } = default!;
    public string? Category { get; set; }
    public string? Title { get; set; }
    public bool Adult { get; set; }
    public int Year { get; set; }
}
