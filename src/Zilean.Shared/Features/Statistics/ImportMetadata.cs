namespace Zilean.Shared.Features.Statistics;

public class ImportMetadata
{
    [Key]
    public string Key { get; set; } = default!;

    public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
}
