namespace Zilean.Database.Services;

public interface IImdbFileService
{
    void AddImdbFile(ImdbFile imdbFile);
    Task StoreImdbFiles();

    Task<ImdbSearchResult[]> SearchForImdbIdAsync(string query, int? year = null, string? category = null);
    Task SetImdbLastImportAsync(ImdbLastImport imdbLastImport);
    Task<ImdbLastImport?> GetImdbLastImportAsync(CancellationToken cancellationToken);
    int ImdbFileCount { get; }
}
