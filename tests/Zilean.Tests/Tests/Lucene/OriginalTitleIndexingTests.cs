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
        // Arrange: index a doc with title="the boat" and originalTitle="das boot"
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0082096", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "the boat", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 1981, Field.Store.YES),
        };

        // D-04: Only add originalTitle if it differs from title
        var originalTitle = "das boot";
        if (!string.Equals("the boat", originalTitle, StringComparison.Ordinal))
        {
            doc.Add(new TextField(LuceneIndexEntry.OriginalTitle, originalTitle, Field.Store.YES));
        }

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Assert: document has both title and originalTitle fields
        var allDocs = searcher.Search(new MatchAllDocsQuery(), 10);
        var storedDoc = searcher.Doc(allDocs.ScoreDocs[0].Doc);
        storedDoc.Get(LuceneIndexEntry.Title).Should().Be("the boat");
        storedDoc.Get(LuceneIndexEntry.OriginalTitle).Should().Be("das boot");

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
        // Arrange: index a doc with title="the boat" and originalTitle="das boot"
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0082096", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "the boat", Field.Store.YES),
            new TextField(LuceneIndexEntry.OriginalTitle, "das boot", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 1981, Field.Store.YES),
        };

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Act: search for "das boot" using dual FuzzyQuery with SHOULD
        var titleQuery = new BooleanQuery { MinimumNumberShouldMatch = 1 };
        var fuzzyTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.Title, "das boot"), 2, 1, 1, false);
        titleQuery.Add(fuzzyTitleQuery, Occur.SHOULD);
        var fuzzyOriginalTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.OriginalTitle, "das boot"), 2, 1, 1, false);
        titleQuery.Add(fuzzyOriginalTitleQuery, Occur.SHOULD);

        var query = new BooleanQuery();
        query.Add(titleQuery, Occur.MUST);
        var categoryQuery = new TermQuery(new Term(LuceneIndexEntry.Category, "movie"));
        query.Add(categoryQuery, Occur.MUST);

        var results = searcher.Search(query, 10);

        // Assert: should find the document via originalTitle match
        results.TotalHits.Should().BeGreaterThan(0,
            "dual FuzzyQuery should match on originalTitle when primaryTitle doesn't match");

        var matchedDoc = searcher.Doc(results.ScoreDocs[0].Doc);
        matchedDoc.Get(LuceneIndexEntry.ImdbId).Should().Be("tt0082096");

        reader.Dispose();
    }

    [Fact]
    public void DualFuzzyQuery_StillMatchesOnPrimaryTitle()
    {
        // Arrange: index a doc with title="the boat" and originalTitle="das boot"
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0082096", Field.Store.YES),
            new TextField(LuceneIndexEntry.Title, "the boat", Field.Store.YES),
            new TextField(LuceneIndexEntry.OriginalTitle, "das boot", Field.Store.YES),
            new StringField(LuceneIndexEntry.Category, "movie", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 1981, Field.Store.YES),
        };

        session.Writer.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Act: search for "the boat" using dual FuzzyQuery
        var titleQuery = new BooleanQuery { MinimumNumberShouldMatch = 1 };
        var fuzzyTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.Title, "the boat"), 2, 1, 1, false);
        titleQuery.Add(fuzzyTitleQuery, Occur.SHOULD);
        var fuzzyOriginalTitleQuery = new FuzzyQuery(new Term(LuceneIndexEntry.OriginalTitle, "the boat"), 2, 1, 1, false);
        titleQuery.Add(fuzzyOriginalTitleQuery, Occur.SHOULD);

        var query = new BooleanQuery();
        query.Add(titleQuery, Occur.MUST);
        var categoryQuery = new TermQuery(new Term(LuceneIndexEntry.Category, "movie"));
        query.Add(categoryQuery, Occur.MUST);

        var results = searcher.Search(query, 10);

        // Assert: should still find the document via primaryTitle
        results.TotalHits.Should().BeGreaterThan(0,
            "dual FuzzyQuery should still match on primaryTitle (no regression)");

        reader.Dispose();
    }
}
