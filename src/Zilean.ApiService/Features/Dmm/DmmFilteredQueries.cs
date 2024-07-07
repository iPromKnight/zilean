namespace Zilean.ApiService.Features.Dmm;

public static class DmmFilteredQueries
{
    public static Func<QueryContainerDescriptor<ExtractedDmmEntry>, QueryContainer> PerformElasticSearchFiltered(string query, int? season, int? episode)
    {
        var matchQueries = BuildMatchQueries(query, season, episode);

        return q => q.Bool(b => b.Must(matchQueries));
    }

    public static Func<QueryContainerDescriptor<ExtractedDmmEntry>, QueryContainer> PerformUnfilteredSearch(DmmQueryRequest queryRequest) =>
        q => q.Match(t =>
        t.Field(f => f.Filename)
            .Query(queryRequest.QueryText));


    private static QueryContainer[] BuildMatchQueries(string query, int? season, int? episode)
    {
        var mustQueries = new List<QueryContainer>
        {
            MatchQueryAsPhrase(query, 10.0)
        };

        if (!episode.HasValue && season.HasValue)
        {
            mustQueries.Add(MatchExactSeasonOrHyphen(season.Value, 13.0));
        }

        if (episode.HasValue && season.HasValue)
        {
            mustQueries.Add(MatchExactSeasonAndEpisode(season.Value, episode.Value, 13.0));
        }

        return [.. mustQueries];
    }

    private static QueryContainer MatchQueryAsPhrase(string qry, double boost) =>
        new QueryContainerDescriptor<ExtractedDmmEntry>().MatchPhrase(mp => mp
            .Field(f => f.Filename)
            .Query(qry)
            .Boost(boost));

    private static QueryContainer MatchExactSeasonOrHyphen(int season, double boost)
    {
        string seasonPattern = $"S{season:00}";
        string seasonTextPattern = $"Season {season}";
        string seasonPatternWithHyphen = $"S{season:00}-";

        return new QueryContainerDescriptor<ExtractedDmmEntry>().Bool(b => b
            .Should(
                s => s.MatchPhrase(mp => mp
                    .Field(f => f.Filename)
                    .Query(seasonPattern)
                    .Boost(boost)),
                s => s.MatchPhrase(mp => mp
                    .Field(f => f.Filename)
                    .Query(seasonTextPattern)
                    .Boost(boost)),
                s => s.MatchPhrasePrefix(mpp => mpp
                    .Field(f => f.Filename)
                    .Query(seasonPatternWithHyphen)
                    .Boost(boost)))
            .MinimumShouldMatch(1));
    }

    private static QueryContainer MatchExactSeasonAndEpisode(int season, int episode, double boost)
    {
        string seasonAndEpisodePattern = $"S{season:00}E{episode:00}";

        return new QueryContainerDescriptor<ExtractedDmmEntry>().Bool(b => b
            .Should(
                s => s.MatchPhrase(mp => mp
                    .Field(f => f.Filename)
                    .Query(seasonAndEpisodePattern)
                    .Boost(boost))));
    }
}
