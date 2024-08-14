namespace Zilean.Database.Services;

public class TorrentInfoResult
{
    public string InfoHash { get; set; } = default!;
    public string[] Resolution { get; set; } = [];
    public int? Year { get; set; }
    public bool Remastered { get; set; }
    public string[] Codec { get; set; } = [];
    public string[] Audio { get; set; } = [];
    public string[] Quality { get; set; } = [];
    public int[] Episodes { get; set; } = [];
    public int[] Seasons { get; set; } = [];
    public string[] Languages { get; set; } = [];
    public string? Title { get; set; }
    public string? RawTitle { get; set; }
    public long Size { get; set; }
    public string Category { get; set; } = default!;
    public double Score { get; set; }
    public string? ImdbId { get; set; }

    // Aliased columns
    public string? ImdbCategory { get; set; } // Matches the alias in SQL
    public string? ImdbTitle { get; set; }    // Matches the alias in SQL
    public int? ImdbYear { get; set; }        // Matches the alias in SQL
    public bool ImdbAdult { get; set; }       // Matches the alias in SQL
}
