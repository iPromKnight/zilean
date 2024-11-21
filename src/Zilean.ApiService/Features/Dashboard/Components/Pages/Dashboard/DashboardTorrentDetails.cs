namespace Zilean.ApiService.Features.Dashboard.Components.Pages.Dashboard;

public class DashboardTorrentDetails
{
    [Required]
    [StringLength(40)]
    public string InfoHash { get; set; } = default!;
    public string Category { get; set; } = default!;
    [Required]
    public string? RawTitle { get; set; }
    public string? ParsedTitle { get; set; }
    public bool? Trash { get; set; } = false;
    [Range(0, 9999, ErrorMessage = "Please enter valid Year between 0 and 9999")]
    public string? Year { get; set; }
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Please enter valid Filesize in Bytes")]
    public string? Size { get; set; }
    public string? ImdbId { get; set; }
    public bool IsAdult { get; set; }
    public bool ChangeCategory { get; set; }
    public bool ChangeTrash { get; set; }
    public bool ChangeYear { get; set; }
    public bool ChangeAdult { get; set; }
    public bool ChangeImdb { get; set; }

    public static TorrentInfo ToTorrentInfo(DashboardTorrentDetails dtd) => new()
    {
        InfoHash = dtd.InfoHash,
        RawTitle = dtd.RawTitle,
        ParsedTitle = dtd.ParsedTitle,
        Trash = dtd.Trash,
        Year = !dtd.Year.IsNullOrWhiteSpace() ? int.Parse(dtd.Year) : null,
        Category = dtd.Category,
        Size = dtd.Size,
        ImdbId = dtd.ImdbId,
        IsAdult = dtd.IsAdult
    };

    public static DashboardTorrentDetails FromTorrentInfo(TorrentInfo ti) => new()
    {
        InfoHash = ti.InfoHash,
        RawTitle = ti.RawTitle,
        ParsedTitle = ti.ParsedTitle,
        Trash = ti.Trash,
        Year = ti.Year.HasValue ? ti.Year.ToString() : null,
        Category = ti.Category,
        Size = ti.Size,
        ImdbId = ti.ImdbId,
        IsAdult = ti.IsAdult
    };
}
