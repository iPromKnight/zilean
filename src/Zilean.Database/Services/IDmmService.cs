namespace Zilean.Database.Services;

public interface IDmmService
{
    Task<DmmLastImport?> GetDmmLastImportAsync(CancellationToken cancellationToken);
    Task SetDmmImportAsync(DmmLastImport dmmLastImport);
    Task AddPagesToIngestedAsync(IEnumerable<ParsedPages> pageNames);
    Task<IEnumerable<ParsedPages>> GetIngestedPagesAsync();
}
