using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Zilean.Database.Dtos;

namespace Zilean.Tests.Tests.Lucene;

public class OriginalTitleIndexingTests
{
    [Fact]
    public void Document_WithDifferentOriginalTitle_HasBothTitleFields()
    {
        // Arrange: "La vita è bella" (Italian) vs "Life Is Beautiful" (English) — completely different strings
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0118799", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "life is beautiful", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 1997, Field.Store.YES),
        };

        var originalTitle = "la vita e bella";
        if (!string.Equals("life is beautiful", originalTitle, StringComparison.Ordinal))
        {
            doc.Add(new TextField(LuceneIndexEntry.OriginalTitle, originalTitle, Field.Store.YES));
        }

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        var allDocs = searcher.Search(new MatchAllDocsQuery(), 10);
        var storedDoc = searcher.Doc(allDocs.ScoreDocs[0].Doc);
        storedDoc.Get(LuceneIndexEntry.Title).Should().Be("life is beautiful");
        storedDoc.Get(LuceneIndexEntry.OriginalTitle).Should().Be("la vita e bella");

        reader.Dispose();
    }

    [Fact]
    public void Document_WithSameOriginalTitle_HasOnlyTitleField()
    {
        // Arrange: index a doc where originalTitle equals title
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0903747", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "breaking bad", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "tvseries", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 2008, Field.Store.YES),
        };

        // D-04: Don't add originalTitle if same as title
        var originalTitle = "breaking bad";
        if (!string.IsNullOrWhiteSpace(originalTitle) &&
            !string.Equals("breaking bad", originalTitle, StringComparison.Ordinal))
        {
            doc.Add(new TextField(LuceneIndexEntry.OriginalTitle, originalTitle, Field.Store.YES));
        }

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Assert: document has NO originalTitle field
        var allDocs = searcher.Search(new MatchAllDocsQuery(), 10);
        var storedDoc = searcher.Doc(allDocs.ScoreDocs[0].Doc);
        storedDoc.Get(LuceneIndexEntry.OriginalTitle).Should().BeNull();

        reader.Dispose();
    }

    [Fact]
    public void Document_WithNullOriginalTitle_HasOnlyTitleField()
    {
        // Arrange: index a doc with null originalTitle
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0082096", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "the boat", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 1981, Field.Store.YES),
        };

        // null originalTitle: don't add the field
        string? originalTitle = null;
        if (!string.IsNullOrWhiteSpace(originalTitle) &&
            !string.Equals("the boat", originalTitle, StringComparison.Ordinal))
        {
            doc.Add(new TextField(LuceneIndexEntry.OriginalTitle, originalTitle, Field.Store.YES));
        }

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        var allDocs = searcher.Search(new MatchAllDocsQuery(), 10);
        var storedDoc = searcher.Doc(allDocs.ScoreDocs[0].Doc);
        storedDoc.Get(LuceneIndexEntry.OriginalTitle).Should().BeNull();

        reader.Dispose();
    }

    [Fact]
    public void DualFuzzyQuery_MatchesOnOriginalTitle_WhenPrimaryTitleDiffers()
    {
        // Arrange: "Cidade de Deus" (Portuguese) vs "City of God" (English)
        // A torrent named "cidade" should match via originalTitle even though primaryTitle is "city"
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0317248", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "city of god", Field.Store.YES),
            new TextField(LuceneIndexEntry.OriginalTitle, "cidade de deus", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 2002, Field.Store.YES),
        };

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Act: search for "cidade" — no fuzzy match possible on "city" (too different)
        var titleQuery = new BooleanQuery { MinimumNumberShouldMatch = 1 };
        var fuzzyTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.Title, "cidade"), 2, 1, 1, false);
        titleQuery.Add(fuzzyTitleQuery, Occur.SHOULD);
        var fuzzyOriginalTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.OriginalTitle, "cidade"), 2, 1, 1, false);
        titleQuery.Add(fuzzyOriginalTitleQuery, Occur.SHOULD);

        var query = new BooleanQuery();
        query.Add(titleQuery, Occur.MUST);
        var categoryQuery = new TermQuery(new Term(LuceneIndexEntry.Category, "movie"));
        query.Add(categoryQuery, Occur.MUST);

        var results = searcher.Search(query, 10);

        // Assert: found via originalTitle — "cidade" matches "cidade" exactly in the original_title field
        results.TotalHits.Should().BeGreaterThan(0,
            "dual FuzzyQuery should match on originalTitle when primaryTitle is completely different");

        var matchedDoc = searcher.Doc(results.ScoreDocs[0].Doc);
        matchedDoc.Get(LuceneIndexEntry.ImdbId).Should().Be("tt0317248");

        reader.Dispose();
    }

    [Fact]
    public void DualFuzzyQuery_StillMatchesOnPrimaryTitle()
    {
        // Arrange: "Der Untergang" (German) vs "Downfall" (English)
        // Searching "downfall" should still match via primaryTitle
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0363163", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "downfall", Field.Store.YES),
            new TextField(LuceneIndexEntry.OriginalTitle, "der untergang", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 2004, Field.Store.YES),
        };

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Act: search for "downfall" — should match on primaryTitle despite originalTitle being completely different
        var titleQuery = new BooleanQuery { MinimumNumberShouldMatch = 1 };
        var fuzzyTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.Title, "downfall"), 2, 1, 1, false);
        titleQuery.Add(fuzzyTitleQuery, Occur.SHOULD);
        var fuzzyOriginalTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.OriginalTitle, "downfall"), 2, 1, 1, false);
        titleQuery.Add(fuzzyOriginalTitleQuery, Occur.SHOULD);

        var query = new BooleanQuery();
        query.Add(titleQuery, Occur.MUST);
        var categoryQuery = new TermQuery(new Term(LuceneIndexEntry.Category, "movie"));
        query.Add(categoryQuery, Occur.MUST);

        var results = searcher.Search(query, 10);

        // Assert: still matches via primaryTitle (no regression)
        results.TotalHits.Should().BeGreaterThan(0,
            "dual FuzzyQuery should still match on primaryTitle (no regression)");

        reader.Dispose();
    }
}
