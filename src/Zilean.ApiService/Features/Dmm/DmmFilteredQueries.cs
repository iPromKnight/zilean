namespace Zilean.ApiService.Features.Dmm;

public static class DmmFilteredQueries
{
    public static Func<QueryContainerDescriptor<TorrentInfo>, QueryContainer> PerformElasticSearchFiltered(DmmFilteredRequest request)
    {
        var titleQueries = new List<QueryContainer>
        {
            MatchQueryAsPhrase(request.Query, 10.0),
            MatchQueryAsFuzzy(request.Query)
        };

        var mustQueries = new List<QueryContainer>
        {
            new QueryContainerDescriptor<TorrentInfo>().Bool(b => b
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

    public static Func<QueryContainerDescriptor<TorrentInfo>, QueryContainer> PerformUnfilteredSearch(DmmQueryRequest queryRequest) =>
        q => q.Bool(b => b
            .Should(
                MatchQueryAsPhrase(queryRequest.QueryText, 10.0),
                MatchQueryAsFuzzy(queryRequest.QueryText))
            .MinimumShouldMatch(1));

    private static QueryContainer[] BuildShouldQueries(DmmFilteredRequest request)
    {
        var shouldQueries = new List<QueryContainer>();

       HandleSeasonAndEpisode(request, shouldQueries);

        if (request.Year.HasValue)
        {
            shouldQueries.Add(MatchYearRange(request.Year.Value));
        }

        if (!string.IsNullOrEmpty(request.Language))
        {
            shouldQueries.Add(MatchLanguage(request.Language));
        }

        if (!string.IsNullOrEmpty(request.Resolution))
        {
            shouldQueries.Add(MatchResolution(request.Resolution));
        }

        return [.. shouldQueries];
    }

    private static void HandleSeasonAndEpisode(DmmFilteredRequest request, List<QueryContainer> shouldQueries)
    {
        if (request.Season.HasValue)
        {
            shouldQueries.Add(MatchSeason(request.Season.Value));

            if (request.Episode.HasValue)
            {
                shouldQueries.Add(new QueryContainerDescriptor<TorrentInfo>().Bool(b => b
                    .Must(
                        MatchSeason(request.Season.Value),
                        MatchEpisode(request.Episode.Value)
                    )));
            }

            return;
        }

        if (!request.Season.HasValue)
        {
            if (request.Episode.HasValue)
            {
                shouldQueries.Add(new QueryContainerDescriptor<TorrentInfo>().Bool(b => b
                    .Must(
                        MatchSeason(0),
                        MatchEpisode(request.Episode.Value)
                    )));
            }
        }
    }

    private static QueryContainer MatchQueryAsPhrase(string qry, double boost) =>
        new QueryContainerDescriptor<TorrentInfo>().MatchPhrase(mp => mp
            .Field(f => f.Title)
            .Query(qry)
            .Boost(boost));

    private static QueryContainer MatchQueryAsFuzzy(string qry) =>
        new QueryContainerDescriptor<TorrentInfo>().Fuzzy(fz => fz
            .Field(f => f.Title)
            .Value(qry)
            .Fuzziness(Fuzziness.Auto));

    private static QueryContainer MatchSeason(int season) =>
        new QueryContainerDescriptor<TorrentInfo>().TermsSet(ts => ts
            .Field(f => f.Seasons)
            .Terms(season)
            .MinimumShouldMatchScript(msm => msm
                .Source("1")));

    private static QueryContainer MatchEpisode(int episode) =>
        new QueryContainerDescriptor<TorrentInfo>().TermsSet(ts => ts
            .Field(f => f.Episodes)
            .Terms(episode)
            .MinimumShouldMatchScript(msm => msm
                .Source("1")));

    private static QueryContainer MatchYearRange(int year) =>
        new QueryContainerDescriptor<TorrentInfo>().Range(r => r
            .Field(f => f.Year)
            .GreaterThanOrEquals(year - 1)
            .LessThanOrEquals(year + 1));

    private static QueryContainer MatchLanguage(string language) =>
        new QueryContainerDescriptor<TorrentInfo>().TermsSet(t => t
            .Field(f => f.Languages)
            .Terms(language)
            .MinimumShouldMatchScript(msm => msm
                .Source("1")));

    private static QueryContainer MatchResolution(string resolution) =>
        new QueryContainerDescriptor<TorrentInfo>().Term(t => t
            .Field(f => f.Resolution)
            .Value(resolution));
}
