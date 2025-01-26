namespace Zilean.Database.Services.FuzzyString;

public static partial class ImdbFuzzyStringMatchingServiceLogger
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "No suitable match found for Torrent '{Title}', Category: {Category}")]
    public static partial void NoSuitableMatchFound(this ILogger logger, string title, string category);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Torrent '{Title}' updated from IMDb ID '{OldImdbId}' to '{NewImdbId}' with a score of {Score}, Category: {Category}, Imdb Title: {ImdbTitle}, Imdb Year: {ImdbYear}")]
    public static partial void TorrentUpdated(
        this ILogger logger,
        string title,
        string oldImdbId,
        string newImdbId,
        double score,
        string category,
        string imdbTitle,
        int imdbYear);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Torrent '{Title}' retained its existing IMDb ID '{ImdbId}' with a best match score of {Score}, Category: {Category}, Imdb Title: {ImdbTitle}, Imdb Year: {ImdbYear}")]
    public static partial void TorrentRetained(
        this ILogger logger,
        string title,
        string imdbId,
        double score,
        string category,
        string imdbTitle,
        int imdbYear);
}
