namespace Zilean.Shared.Features.Torznab.Info;

public class ReleaseInfo() : ICloneable
{
    public const long Seeders = 999;
    public const long Peers = 999;
    public const string Origin = "Zilean";
    public string? Title { get; set; }
    public Uri? Guid { get; set; }
    public Uri? Magnet { get; set; }
    public Uri? Details { get; set; }
    public DateTime PublishDate { get; set; }
    public ICollection<int> Category { get; set; } = [];
    public long? Size { get; set; }
    public string? Description { get; set; }
    public long? Imdb { get; set; }
    public ICollection<string> Languages { get; set; } = [];
    public long? Year { get; set; }
    public string? InfoHash { get; set; }
    public static double? GigabytesFromBytes(double? size) => size / 1024.0 / 1024.0 / 1024.0;

    private ReleaseInfo(ReleaseInfo copyFrom) : this()
    {
        Title = copyFrom.Title;
        Guid = copyFrom.Guid;
        Magnet = copyFrom.Magnet;
        Details = copyFrom.Details;
        PublishDate = copyFrom.PublishDate;
        Category = copyFrom.Category;
        Size = copyFrom.Size;
        Description = copyFrom.Description;
        Imdb = copyFrom.Imdb;
        Languages = copyFrom.Languages;
        Year = copyFrom.Year;
        InfoHash = copyFrom.InfoHash;
    }

    public virtual object Clone() => new ReleaseInfo(this);

    public override string ToString() =>
        $"[ReleaseInfo: Title={Title}, Guid={Guid}, Link={Magnet}, Details={Details}, PublishDate={PublishDate}, Category={Category}, Size={Size}, Description={Description}, Imdb={Imdb}, Seeders={Seeders}, Peers={Peers}, InfoHash={InfoHash}]";
}
