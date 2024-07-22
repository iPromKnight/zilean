namespace Zilean.Shared.Features.Imdb;

public class ImdbFile
{
    public string ImdbId { get; set; } = default!;
    public string? Category { get; set; }
    public string? Title { get; set; }
    public bool Adult { get; set; }
    public int Year { get; set; }

    public static IClrTypeMapping<ImdbFile> ImdbFileDefaultMapping(ClrTypeMappingDescriptor<ImdbFile> x)
    {
        x.IndexName(ElasticSearchClient.ImdbMetadataIndex);
        x.IdProperty(p => p.ImdbId);

        return x;
    }

    public static ITypeMapping ImdbFileIndexMapping(TypeMappingDescriptor<ImdbFile> x)
    {
        x.Properties(prop =>
        {
            prop.Text(s => s.Name(p => p.Title));
            prop.Number(s => s.Name(p => p.Year).Type(NumberType.Integer));
            prop.Boolean(s => s.Name(p => p.Adult));
            prop.Text(s => s.Name(p => p.Category));
            prop.Keyword(s => s.Name(p => p.ImdbId));
            return prop;
        });

        return x;
    }
}
