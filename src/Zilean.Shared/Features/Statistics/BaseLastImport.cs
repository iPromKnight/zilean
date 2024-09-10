namespace Zilean.Shared.Features.Statistics;

public abstract class BaseLastImport
{
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;
    public ImportStatus Status { get; set; } = ImportStatus.InProgress;
}
