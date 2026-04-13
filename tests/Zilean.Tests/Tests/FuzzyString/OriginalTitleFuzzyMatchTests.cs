using Zilean.Database.Services.FuzzyString;

namespace Zilean.Tests.Tests.FuzzyString;

public class OriginalTitleFuzzyMatchTests
{
    private const double ExactMatchTitleYearScore = 2.0;
    private const double CloseMatchTitleYearScore = 1.5;

    [Fact]
    public void CalculateScore_ExactMatchOnOriginalTitle_ReturnsExactMatchScore()
    {
        // Arrange
        var torrent = new TorrentInfo { ParsedTitle = "das boot", Year = 1981 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0082096",
            Title = "the boat",
            OriginalTitle = "das boot",
            Year = 1981,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: exact match on OriginalTitle + year
        score.Should().Be(ExactMatchTitleYearScore * 100);
    }

    [Fact]
    public void CalculateScore_CloseYearMatchOnOriginalTitle_ReturnsCloseMatchScore()
    {
        // Arrange
        var torrent = new TorrentInfo { ParsedTitle = "das boot", Year = 1982 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0082096",
            Title = "the boat",
            OriginalTitle = "das boot",
            Year = 1981,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: close match on OriginalTitle (year within 1)
        score.Should().Be(CloseMatchTitleYearScore * 100);
    }

    [Fact]
    public void CalculateScore_OriginalTitleCloserFuzzyMatch_ReturnsHigherScore()
    {
        // Arrange: torrent title is closer to OriginalTitle than Title
        var torrent = new TorrentInfo { ParsedTitle = "das boot", Year = 2020 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0082096",
            Title = "the boat",
            OriginalTitle = "das boote",
            Year = 1981,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: score should be based on OriginalTitle fuzzy match (higher than Title fuzzy match)
        var titleOnlyScore = ImdbFuzzyStringMatchingService.CalculateSingleTitleScore(torrent, imdb.Title, imdb.Year);
        score.Should().BeGreaterThan(titleOnlyScore);
    }

    [Fact]
    public void CalculateScore_NullOriginalTitle_ReturnsTitleBasedScore()
    {
        // Arrange
        var torrent = new TorrentInfo { ParsedTitle = "the boat", Year = 1981 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0082096",
            Title = "the boat",
            OriginalTitle = null,
            Year = 1981,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: should match on Title with exact match score
        score.Should().Be(ExactMatchTitleYearScore * 100);
    }

    [Fact]
    public void CalculateScore_TitleBetterMatch_ReturnsTitleScore()
    {
        // Arrange: torrent title matches Title better than OriginalTitle
        var torrent = new TorrentInfo { ParsedTitle = "the boat", Year = 1981 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0082096",
            Title = "the boat",
            OriginalTitle = "das boot",
            Year = 1981,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: should return title-based exact match score (higher than originalTitle fuzzy)
        score.Should().Be(ExactMatchTitleYearScore * 100);
    }
}
