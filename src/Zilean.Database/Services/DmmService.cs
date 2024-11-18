using System.Text.Json;

namespace Zilean.Database.Services;

public class DmmService(ILogger<DmmService> logger, ZileanConfiguration configuration, IServiceProvider serviceProvider) : BaseDapperService(logger, configuration)
{
    public async Task<DmmLastImport?> GetDmmLastImportAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var dmmLastImport = await dbContext.ImportMetadata.AsNoTracking().FirstOrDefaultAsync(x => x.Key == MetadataKeys.DmmLastImport, cancellationToken: cancellationToken);

        return dmmLastImport?.Value.Deserialize<DmmLastImport>();
    }

    public async Task SetDmmImportAsync(DmmLastImport dmmLastImport)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        var metadata = await dbContext.ImportMetadata.FirstOrDefaultAsync(x => x.Key == MetadataKeys.DmmLastImport);

        if (metadata is null)
        {
            metadata = new ImportMetadata
            {
                Key = MetadataKeys.DmmLastImport,
                Value = JsonSerializer.SerializeToDocument(dmmLastImport),
            };
            await dbContext.ImportMetadata.AddAsync(metadata);
            await dbContext.SaveChangesAsync();
            return;
        }

        metadata.Value = JsonSerializer.SerializeToDocument(dmmLastImport);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddPagesToIngestedAsync(IEnumerable<ParsedPages> pageNames, CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await dbContext.ParsedPages.AddRangeAsync(pageNames, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPageToIngestedAsync(ParsedPages pageNames, CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await dbContext.ParsedPages.AddAsync(pageNames, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ParsedPages>> GetIngestedPagesAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        return await dbContext.ParsedPages.AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
    }
}
