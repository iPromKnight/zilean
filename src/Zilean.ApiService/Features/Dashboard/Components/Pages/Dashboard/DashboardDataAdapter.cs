namespace Zilean.ApiService.Features.Dashboard.Components.Pages.Dashboard;

public class DashboardDataAdapter(IServiceProvider serviceProvider, ParseTorrentNameService parseTorrentNameService, ILogger<DashboardDataAdapter> logger) : DataAdaptor
{
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? key = null)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            var dataSource = dbContext
                .Torrents
                .AsNoTracking()
                .OrderByDescending(x => x.IngestedAt)
                .AsQueryable();

            if (dataManagerRequest.Search is { Count: > 0 })
            {
                dataSource = DataOperations.PerformSearching(dataSource, dataManagerRequest.Search);
            }

            if (dataManagerRequest.Where is { Count: > 0 })
            {
                dataSource = DataOperations.PerformFiltering(dataSource, dataManagerRequest.Where,
                    dataManagerRequest.Where[0].Operator);
            }

            if (dataManagerRequest.Sorted is { Count: > 0 })
            {
                dataSource = DataOperations.PerformSorting(dataSource, dataManagerRequest.Sorted);
            }

            int count = dataSource.Count();

            if (dataManagerRequest.Skip != 0)
            {
                dataSource = DataOperations.PerformSkip(dataSource, dataManagerRequest.Skip);
            }

            if (dataManagerRequest.Take != 0)
            {
                dataSource = DataOperations.PerformTake(dataSource, dataManagerRequest.Take);
            }

            var results = await dataSource
                .Select(x => DashboardTorrentDetails.FromTorrentInfo(x))
                .ToListAsync();

            return !dataManagerRequest.RequiresCounts
                ? results
                : new DataResult
            {
                Result = results,
                Count = count,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error reading data");
            throw;
        }
    }

    public override async Task<object> InsertAsync(DataManager dataManager, object? value, string key)
    {
        try
        {
            if (value is not DashboardTorrentDetails incoming)
            {
                return null;
            }

            await using var scope = serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            var torrent = DashboardTorrentDetails.ToTorrentInfo(incoming);
            torrent = await UpdateTorrentAttributes(incoming, torrent, true);

            await dbContext.Torrents.AddAsync(torrent);
            await dbContext.SaveChangesAsync();
            return value;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error inserting data");
            throw;
        }
    }

    public override async Task<object> UpdateAsync(DataManager dataManager, object? value, string keyField, string key)
    {
        try
        {
            if (value is not DashboardTorrentDetails incoming)
            {
                return null;
            }

            await using var scope = serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            var torrent = await dbContext.Torrents.AsNoTracking().FirstOrDefaultAsync(x=> x.InfoHash == incoming.InfoHash);
            if (torrent == null)
            {
                return null;
            }

            torrent.RawTitle = incoming.RawTitle;
            torrent.Size = incoming.Size;
            torrent = await UpdateTorrentAttributes(incoming, torrent);

            dbContext.Torrents.Update(torrent);
            await dbContext.SaveChangesAsync();
            return value;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating data");
            throw;
        }
    }

    public override async Task<object> RemoveAsync(DataManager dataManager, object? value, string keyField, string key)
    {
        try
        {
            if (value is not string incoming)
            {
                return null;
            }

            await using var scope = serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            var result = await dbContext.Torrents.Where(x=>x.InfoHash == incoming).ExecuteDeleteAsync();
            return result == 0 ? throw new InvalidOperationException("No records were deleted") : value;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error removing data");
            throw;
        }
    }

    public override async Task<object> BatchUpdateAsync(DataManager dataManager, object? changed, object? added, object? deleted,
        string keyField, string key, int? dropIndex)
    {
        try
        {
            if (deleted == null)
            {
                return key;
            }

            await using var scope = serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

            var deletedIds = ((IEnumerable<DashboardTorrentDetails>)deleted).Select(x => x.InfoHash).ToList();
            var result = await dbContext.Torrents.Where(x => deletedIds.Contains(x.InfoHash)).ExecuteDeleteAsync();
            return result == 0
                ? throw new InvalidOperationException("No records were deleted")
                : result != deletedIds.Count ? throw new InvalidOperationException("Not all records were deleted") : (object)key;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error batch updating data");
            throw;
        }
    }

    private async Task<TorrentInfo> UpdateTorrentAttributes(DashboardTorrentDetails incoming, TorrentInfo torrent, bool isCreate = false)
    {
        torrent = await parseTorrentNameService.ParseAndPopulateTorrentInfoAsync(torrent);
        torrent.CleanedParsedTitle = Parsing.CleanQuery(torrent.ParsedTitle);

        if (isCreate)
        {
            torrent.IngestedAt = DateTime.UtcNow;
            return torrent;
        }

        if (incoming.ChangeCategory || (!incoming.Category.IsNullOrWhiteSpace() && incoming.Category != torrent.Category))
        {
            torrent.Category = incoming.Category;
        }

        if (incoming.ChangeTrash || incoming.Trash != torrent.Trash)
        {
            torrent.Trash = incoming.Trash;
        }

        if (incoming.ChangeYear || (!incoming.Year.IsNullOrWhiteSpace() && incoming.Year != torrent.Year.ToString()))
        {
            torrent.Year = incoming.Year.IsNullOrWhiteSpace() ? null : int.Parse(incoming.Year);
        }

        if (incoming.ChangeAdult || incoming.IsAdult != torrent.IsAdult)
        {
            torrent.IsAdult = incoming.IsAdult;
        }

        if (incoming.ChangeImdb || incoming.ImdbId != torrent.ImdbId)
        {
            torrent.ImdbId = incoming.ImdbId;
        }

        return torrent;
    }
}
