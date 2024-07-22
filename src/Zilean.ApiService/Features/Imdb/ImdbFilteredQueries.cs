namespace Zilean.ApiService.Features.Imdb;

public static class ImdbFilteredQueries
{
    public static Func<QueryContainerDescriptor<ImdbFile>, QueryContainer> PerformElasticSearchFiltered(ImdbFilteredRequest request)
    {
        var titleQueries = new List<QueryContainer>
        {
            MatchQueryAsPhrase(request.Query, 10.0),
            MatchQueryAsFuzzy(request.Query)
        };

        var mustQueries = new List<QueryContainer>
        {
            new QueryContainerDescriptor<ImdbFile>().Bool(b => b
                .Should(titleQueries.ToArray())
                .MinimumShouldMatch(1))
        };

        var shouldQueries = BuildShouldQueries(request);

        return shouldQueries.Length > 0
            ? (q => q.Bool(b => b
                .Must(mustQueries.ToArray())
                .Should(shouldQueries)
                .MinimumShouldMatch(1)))
            : (q => q.Bool(b => b.Must(mustQueries.ToArray())));
    }

    private static QueryContainer[] BuildShouldQueries(ImdbFilteredRequest request)
    {
        var shouldQueries = new List<QueryContainer>();

        if (request.Year.HasValue)
        {
            shouldQueries.Add(MatchYearRange(request.Year.Value));
        }

        return [.. shouldQueries];
    }

    private static QueryContainer MatchQueryAsPhrase(string qry, double boost) =>
        new QueryContainerDescriptor<ImdbFile>().MatchPhrase(mp => mp
            .Field(f => f.Title)
            .Query(qry)
            .Boost(boost));

    private static QueryContainer MatchQueryAsFuzzy(string qry) =>
        new QueryContainerDescriptor<ImdbFile>().Fuzzy(fz => fz
            .Field(f => f.Title)
            .Value(qry)
            .Fuzziness(Fuzziness.Auto));

    private static QueryContainer MatchYearRange(int year) =>
        new QueryContainerDescriptor<ImdbFile>().Range(r => r
            .Field(f => f.Year)
            .GreaterThanOrEquals(year - 1)
            .LessThanOrEquals(year + 1));
}
