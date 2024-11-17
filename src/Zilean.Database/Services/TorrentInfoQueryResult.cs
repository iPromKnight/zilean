namespace Zilean.Database.Services;

public class TorrentInfoResult : TorrentInfo
{
    // Aliased columns
    public string? ImdbCategory { get; set; } // Matches the alias in SQL
    public string? ImdbTitle { get; set; }    // Matches the alias in SQL
    public int? ImdbYear { get; set; }        // Matches the alias in SQL
    public bool ImdbAdult { get; set; }       // Matches the alias in SQL
}
