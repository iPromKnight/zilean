namespace Zilean.Shared.Features.Dmm;

public class TorrentInfo
{
    public string? Resolution { get; set; }
    public int? Year { get; set; }
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

    public bool IsPossibleMovie { get; set; }

    public static IClrTypeMapping<TorrentInfo> TorrentInfoDefaultMapping(ClrTypeMappingDescriptor<TorrentInfo> x)
    {
        x.IndexName(ElasticSearchClient.DmmIndex);
        x.IdProperty(p => p.InfoHash);

        return x;
    }

    public static ITypeMapping TorrentInfoIndexMapping(TypeMappingDescriptor<TorrentInfo> x)
    {
        x.Properties(prop =>
        {
            prop.Text(s => s.Name(p => p.Resolution));
            prop.Number(s => s.Name(p => p.Year).Type(NumberType.Integer));
            prop.Boolean(s => s.Name(p => p.Remastered));
            prop.Text(s => s.Name(p => p.Source));
            prop.Text(s => s.Name(p => p.Codec));
            prop.Text(s => s.Name(p => p.Group));
            prop.Number(s => s.Name(p => p.Episodes).Type(NumberType.Integer));
            prop.Number(s => s.Name(p => p.Seasons).Type(NumberType.Integer));
            prop.Keyword(s => s.Name(p => p.Languages));
            prop.Text(s => s.Name(p => p.Title));
            prop.Text(s => s.Name(p => p.RawTitle));
            prop.Number(s => s.Name(p => p.Size).Type(NumberType.Long));
            prop.Keyword(s => s.Name(p => p.InfoHash));
            prop.Boolean(s => s.Name(p => p.IsPossibleMovie));
            return prop;
        });

        return x;
    }
}
