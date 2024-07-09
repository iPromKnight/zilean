namespace Zilean.Shared.Features.Dmm;

public class ExtractedDmmEntry(string? infoHash, string? filename, long filesize, TorrentInfo? parseResponse)
{
    public string? Filename { get; set; } = filename;
    public string? InfoHash { get; set; } = infoHash;
    public long Filesize { get; set; } = filesize;
    public TorrentInfo? ParseResponse { get; set; } = parseResponse;
}

public record DmmQueryRequest(string QueryText);
