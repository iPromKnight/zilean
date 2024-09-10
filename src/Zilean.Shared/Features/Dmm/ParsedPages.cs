namespace Zilean.Shared.Features.Dmm;

public class ParsedPages
{
    [Key]
    public string Page { get; set; } = default!;
    public int EntryCount { get; set; }
}
