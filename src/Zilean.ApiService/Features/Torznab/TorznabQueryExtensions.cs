namespace Zilean.ApiService.Features.Torznab;

public static class TorznabQueryExtensions
{
    public static void ValidateQueryAgainstCapabilities(this TorznabQuery query, ILogger logger)
    {
        if (query.ImdbID != null)
        {
            if (query.IsMovieSearch && !TorznabCapabilities.MovieSearchImdbAvailable)
            {
                logger.LogWarning("IMDB ID movie search is not supported.");
                throw new NotSupportedException("IMDB ID movie search is not supported by Zilean.");
            }

            if (query.IsTVSearch && !TorznabCapabilities.TvSearchImdbAvailable)
            {
                logger.LogWarning("IMDB ID TV search is not supported.");
                throw new NotSupportedException("IMDB ID TV search is not supported by Zilean.");
            }

            if (query.IsTVSearch && !TorznabCapabilities.XxxSearchImdbAvailable)
            {
                logger.LogWarning("IMDB ID Xxx search is not supported.");
                throw new NotSupportedException("IMDB ID Xxx search is not supported by Zilean.");
            }
        }

        if (query.Season != null && !TorznabCapabilities.TvSearchSeasonAvailable)
        {
            logger.LogWarning("Season-based TV search is not supported.");
            throw new NotSupportedException("Season-based TV search is not supported by Zilean.");
        }

        if (query.Episode != null && !TorznabCapabilities.TvSearchEpAvailable)
        {
            logger.LogWarning("Episode-based TV search is not supported.");
            throw new NotSupportedException("Episode-based TV search is not supported by Zilean.");
        }

        if (query.Year != null)
        {
            if (query.IsMovieSearch && !TorznabCapabilities.MovieSearchYearAvailable)
            {
                logger.LogWarning("Year-based movie search is not supported.");
                throw new NotSupportedException("Year-based movie search is not supported by Zilean.");
            }

            if (query.IsTVSearch && !TorznabCapabilities.TvSearchYearAvailable)
            {
                logger.LogWarning("Year-based TV search is not supported.");
                throw new NotSupportedException("Year-based TV search is not supported by Zilean.");
            }

            if (query.IsXxxSearch && !TorznabCapabilities.XxxSearchYearAvailable)
            {
                logger.LogWarning("Year-based Xxx search is not supported.");
                throw new NotSupportedException("Year-based Xxx search is not supported by Zilean.");
            }
        }

        if (query.SearchTerm != null && !TorznabCapabilities.SearchAvailable)
        {
            logger.LogWarning("Query text search is not supported.");
            throw new NotSupportedException("Query text search is not supported by Zilean.");
        }

        logger.LogInformation("Query validated successfully against capabilities.");
    }

    public static bool CanHandleQuery(this TorznabQuery query)
    {
        if (query.ImdbID != null)
        {
            if (query.IsMovieSearch && !TorznabCapabilities.MovieSearchImdbAvailable)
            {
                return false;
            }

            if (query.IsTVSearch && !TorznabCapabilities.TvSearchImdbAvailable)
            {
                return false;
            }

            if (query.IsXxxSearch && !TorznabCapabilities.XxxSearchImdbAvailable)
            {
                return false;
            }
        }

        if (query.Season != null && !TorznabCapabilities.TvSearchSeasonAvailable)
        {
            return false;
        }

        if (query.Episode != null && !TorznabCapabilities.TvSearchEpAvailable)
        {
            return false;
        }

        if (query.Year != null)
        {
            if (query.IsMovieSearch && !TorznabCapabilities.MovieSearchYearAvailable)
            {
                return false;
            }

            if (query.IsTVSearch && !TorznabCapabilities.TvSearchYearAvailable)
            {
                return false;
            }

            if (query.IsXxxSearch && !TorznabCapabilities.XxxSearchYearAvailable)
            {
                return false;
            }
        }

        if (query.SearchTerm != null && !TorznabCapabilities.SearchAvailable)
        {
            return false;
        }

        // If all checks pass, the query is supported
        return true;
    }

}
