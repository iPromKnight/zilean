namespace Zilean.ApiService.Features.Dmm;

public class ExtractedDmmEntry(string? infoHash, string? filename, long filesize)
{
    public string? Filename { get; set; } = filename;
    public string? InfoHash { get; set; } = infoHash;
    public long Filesize { get; set; } = filesize;
}

public record DmmQueryRequest(string QueryText);
