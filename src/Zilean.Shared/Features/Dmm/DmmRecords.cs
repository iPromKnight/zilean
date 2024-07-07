namespace Zilean.Shared.Features.Dmm;

public class ExtractedDmmEntry(string? infoHash, string? filename, long filesize, RtnResponse? rtnResponse)
{
    public string? Filename { get; set; } = filename;
    public string? InfoHash { get; set; } = infoHash;
    public long Filesize { get; set; } = filesize;
    public RtnResponse? RtnResponse { get; set; } = rtnResponse;
}

public record DmmQueryRequest(string QueryText);
