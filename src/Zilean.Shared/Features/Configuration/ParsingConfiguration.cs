namespace Zilean.Shared.Features.Configuration;

public class ParsingConfiguration
{
    public bool IncludeAdult { get; set; } = false;
    public bool IncludeTrash { get; set; } = true;
    public int BatchSize { get; set; } = 5000;
}
