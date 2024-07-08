namespace Zilean.Shared.Features.Dmm;

public class TorrentInfo
{
    public string? Resolution { get; set; }
    public string? Year { get; set; }
    public bool Remastered { get; set; }
    public string? Source { get; set; }
    public string? Codec { get; set; }
    public string? Group { get; set; }
    public List<int> Episodes { get; set; } = [];
    public List<int> Seasons { get; set; } = [];
    public List<string> Languages { get; set; } = [];
    public string? Title { get; set; }
    public string? RawTitle { get; set; }
    public long Size { get; set; }
    public string? InfoHash { get; set; }

    public bool IsMovie => Episodes.Count == 0 && Seasons.Count == 0;
}
