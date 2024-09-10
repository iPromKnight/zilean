namespace Zilean.Shared.Features.Dmm;

public class ExtractedDmmEntry(string? infoHash, string? filename, long filesize, TorrentInfo? parseResponse)
{
    public string? Filename { get; set; } = filename;
    public string? InfoHash { get; set; } = infoHash;
    public long Filesize { get; set; } = filesize;
    public TorrentInfo? ParseResponse { get; set; } = parseResponse;
}

public class ExtractedDmmEntryResponse(TorrentInfo torrentInfo)
{
    public string? Filename { get; set; } = torrentInfo.RawTitle;
    public string? InfoHash { get; set; } = torrentInfo.InfoHash;
    public string Filesize { get; set; } = torrentInfo.Size;
    public TorrentInfo ParseResponse { get; set; } = torrentInfo;
}

public record DmmQueryRequest(string QueryText);
