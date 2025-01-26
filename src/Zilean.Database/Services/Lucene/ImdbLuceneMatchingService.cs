using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Zilean.Database.Services.Lucene;

public class ImdbLuceneMatchingService(ILogger<ImdbLuceneMatchingService> logger, ZileanConfiguration configuration) : IImdbMatchingService
{
    private ConcurrentDictionary<string, string?>? _imdbCache;
    private LuceneSession? _imdbFilesIndex;
    private DirectoryReader? _reader;
    private IndexSearcher? _searcher;

    public async Task PopulateImdbData()
    {
        _imdbFilesIndex = await IndexImdbDocumentsInMemory();
        _imdbCache = new();
    }

    public void DisposeImdbData()
    {
        _reader?.Dispose();
        _imdbFilesIndex?.Writer.Dispose();
        _imdbFilesIndex?.Directory.Dispose();
        _imdbFilesIndex?.Dispose();
        _imdbCache.Clear();
        _imdbCache = null;
    }

    public Task<ConcurrentQueue<TorrentInfo>> MatchImdbIdsForBatchAsync(IEnumerable<TorrentInfo> batch)
    {
        if (_imdbFilesIndex is null)
        {
            throw new InvalidOperationException("IMDb data has not been loaded yet.");
        }

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = configuration.Imdb.UseAllCores switch
            {
                true => Environment.ProcessorCount,
                false => configuration.Imdb.NumberOfCores,
            },
        };

        var updatedTorrents = new ConcurrentQueue<TorrentInfo>();

        var groupedByYearAndCategory = batch.GroupBy(
            t => new
            {
                t.Year,
                t.Category,
            });

        _reader = _imdbFilesIndex.Writer.GetReader(applyAllDeletes: true);
        _searcher = new(_reader);

        Parallel.ForEach(
            groupedByYearAndCategory, parallelOptions, (torrentGroup, _) =>
            {
                foreach (var torrent in torrentGroup)
                {
                    if (_imdbCache.TryGetValue(torrent.CacheKey(), out var imdbId))
                    {
                        torrent.ImdbId = imdbId;
                        continue;
                    }

                    var bestMatch = GetBestMatch(torrent);

                    if (bestMatch == null)
                    {
                        logger.NoSuitableMatchFound(torrent.NormalizedTitle, torrent.Category);
                        continue;
                    }

                    if (bestMatch.ImdbId != torrent.ImdbId)
                    {
                        logger.TorrentUpdated(
                            torrent.NormalizedTitle,
                            torrent.ImdbId,
                            bestMatch.ImdbId,
                            bestMatch.Score,
                            torrent.Category,
                            bestMatch.Title,
                            bestMatch.Year);

                        torrent.ImdbId = bestMatch.ImdbId;

                        _imdbCache[torrent.CacheKey()] = bestMatch.ImdbId;

                        updatedTorrents.Enqueue(torrent);
                        continue;
                    }

                    logger.TorrentRetained(
                        torrent.NormalizedTitle,
                        torrent.ImdbId,
                        bestMatch.Score,
                        torrent.Category,
                        bestMatch.Title,
                        bestMatch.Year);
                }
            });

        return Task.FromResult(updatedTorrents);
    }

    private BestMatch? GetBestMatch(TorrentInfo torrent, int maxResults = 10)
    {
        var matches = MatchTitle(torrent, maxResults);

        if (!matches.Any())
        {
            return null;
        }

        BestMatch? bestMatch = null;
        double highestScore = 0;

        foreach (var match in matches)
        {
            if (!(match.Score > highestScore))
            {
                continue;
            }

            highestScore = match.Score;
            bestMatch = match;
        }

        return bestMatch;
    }

    private List<BestMatch> MatchTitle(TorrentInfo torrent, int maxResults = 3)
    {
        if (string.IsNullOrWhiteSpace(torrent.NormalizedTitle))
        {
            return [];
        }

        var combinedQuery = new BooleanQuery();

        AddTitleAndCategoryToQuery(torrent, combinedQuery);

        if (torrent.Year is > 0)
        {
            AddYearToQuery(torrent, combinedQuery);
        }

        var topDocs = _searcher.Search(combinedQuery, maxResults);

        if (topDocs.ScoreDocs.Length == 0)
        {
            return [];
        }

        var results = new List<BestMatch>();

        foreach (var scoreDoc in topDocs.ScoreDocs)
        {
            var doc = _searcher.Doc(scoreDoc.Doc);

            var imdbId = doc.Get(LuceneIndexEntry.ImdbId);
            var title = doc.Get(LuceneIndexEntry.Title);
            var year = doc.GetField(LuceneIndexEntry.Year)?.GetInt32Value() ?? 0;

            results.Add(new(imdbId, title, year, scoreDoc.Score));
        }

        return results;
    }

    private static void AddTitleAndCategoryToQuery(TorrentInfo torrent, BooleanQuery combinedQuery)
    {
        var query = new BooleanQuery();

        var fuzzyTitleQuery = new FuzzyQuery(new(LuceneIndexEntry.Title, torrent.NormalizedTitle), 2, 1, 1, false);
        query.Add(fuzzyTitleQuery, Occur.MUST);

        var categoryQuery = new TermQuery(new(LuceneIndexEntry.Category, torrent.Category.ToLowerInvariant()));
        query.Add(categoryQuery, Occur.MUST);

        combinedQuery.Add(query, Occur.MUST);
    }

    private static void AddYearToQuery(TorrentInfo torrent, BooleanQuery combinedQuery)
    {
        var yearZeroQuery = new TermQuery(new(LuceneIndexEntry.Year, "0"));
        var yearRangeQuery = NumericRangeQuery.NewInt32Range(LuceneIndexEntry.Year, torrent.Year!.Value-1, torrent.Year!.Value+1, true, true);
        var yearQuery = new BooleanQuery
        {
            { yearZeroQuery, Occur.SHOULD },
            { yearRangeQuery, Occur.SHOULD },
        };

        combinedQuery.Add(yearQuery, Occur.SHOULD);
    }

    private async Task<LuceneSession> IndexImdbDocumentsInMemory()
    {
        var luceneSession = LuceneSession.NewInstance();

        logger.LogInformation("Indexing IMDb entries...");

        await using var sqlConnection = new NpgsqlConnection(configuration.Database.ConnectionString);
        await sqlConnection.OpenAsync();

        var imdbFiles = sqlConnection.Query<ImdbFile>(
            """
            SELECT
                "ImdbId",
                Lower(
                unaccent(
                    regexp_replace(
                        regexp_replace(trim("Title"), '\s+', ' ', 'g'), -- Normalize whitespace
                        '[^\w\s]', '', 'g' -- Remove non-alphanumeric characters but keep spaces
                        )
                    )
                ) AS "Title",
                "Adult",
                "Category",
                "Year"
            FROM
                public."ImdbFiles"
            WHERE
                "Category" IN ('tvSeries', 'tvShort', 'tvMiniSeries', 'tvSpecial', 'movie', 'tvMovie')
            """);

        foreach (var imdb in imdbFiles)
        {
            var doc = new Document
            {
                new StringField(LuceneIndexEntry.ImdbId, imdb.ImdbId, Field.Store.YES),
                new StringField(LuceneIndexEntry.Title, imdb.Title, Field.Store.YES),
                new StringField(LuceneIndexEntry.Category, GetCategory(imdb.Category).ToLowerInvariant(), Field.Store.YES),
                new Int32Field(LuceneIndexEntry.Year, imdb.Year, new FieldType
                {
                    IsStored = true,
                    IsIndexed = true,
                    IsTokenized = false,
                    NumericType = NumericType.INT32,
                    IndexOptions = IndexOptions.DOCS_ONLY,
                }),
            };

            luceneSession.Writer.AddDocument(doc);
        }

        luceneSession.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        logger.LogInformation("Indexed {Count} IMDb entries", luceneSession.Writer.NumDocs);

        return luceneSession;
    }

    private static string GetCategory(string? imdbCategory) =>
        imdbCategory switch
        {
            "tvSeries" => "tvSeries",
            "tvShort" => "tvSeries",
            "tvMiniSeries" => "tvSeries",
            "tvSpecial" => "tvSeries",
            "tvMovie" => "movie",
            "movie" => "movie",
            _ => "unknown",
        };


    private record BestMatch(string ImdbId, string Title, int Year, double Score);
}
