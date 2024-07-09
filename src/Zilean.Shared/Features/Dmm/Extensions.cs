namespace Zilean.Shared.Features.Dmm;

public static class Extensions
{
    public static ExtractedDmmEntry ToExtractedDmmEntry(this TorrentInfo torrentInfo) =>
        new(torrentInfo.InfoHash, torrentInfo.RawTitle, torrentInfo.Size, null);
}
