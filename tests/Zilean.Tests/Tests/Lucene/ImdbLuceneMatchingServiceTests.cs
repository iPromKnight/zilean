using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Zilean.Database.Dtos;

namespace Zilean.Tests.Tests.Lucene;

public class ImdbLuceneMatchingServiceTests
{
    [Fact]
    public void FuzzyQuery_MatchesIndividualWords_InMultiWordTitle()
    {
        // Arrange: index a multi-word title using StringField (replicating production bug)
        using var session = LuceneSession.NewInstance();
        var doc = new Document
        {
            new StringField(LuceneIndexEntry.ImdbId, "tt0903747", Field.Store.YES),
            new StringField(LuceneIndexEntry.Title, "breaking bad", Field.Store.YES), // BUG: StringField does not tokenize
            new StringField(LuceneIndexEntry.Category, "tvseries", Field.Store.YES),
            new Int32Field(LuceneIndexEntry.Year, 2008, Field.Store.YES),
        };
        session.Writer!.AddDocument(doc);
        session.Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        using var reader = session.Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        // Act: FuzzyQuery on single word "breakin" (edit distance 2 from "breaking")
        var fuzzyQuery = new FuzzyQuery(new Term(LuceneIndexEntry.Title, "breakin"), 2);
        var results = searcher.Search(fuzzyQuery, 10);

        // Assert: should find the document (fails with StringField because "breaking bad" is one token)
        results.TotalHits.Should().BeGreaterThan(0,
            "FuzzyQuery should match individual words in a tokenized title, not the entire string");
    }

    [Fact]
    public void SearcherAndReader_ShouldBeLocalVariables_NotInstanceFields()
    {
        // Verify the thread-safety fix: _reader and _searcher should not exist as instance fields
        var serviceType = typeof(Database.Services.Lucene.ImdbLuceneMatchingService);
        var readerField = serviceType.GetField("_reader",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var searcherField = serviceType.GetField("_searcher",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        readerField.Should().BeNull("_reader should be a local variable, not an instance field (QUAL-03)");
        searcherField.Should().BeNull("_searcher should be a local variable, not an instance field (QUAL-03)");
    }
}
