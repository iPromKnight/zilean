using Zilean.Database.Services.FuzzyString;

namespace Zilean.Tests.Tests.FuzzyString;

public class OriginalTitleFuzzyMatchTests
{
    private const double ExactMatchTitleYearScore = 2.0;
    private const double CloseMatchTitleYearScore = 1.5;

    [Fact]
    public void CalculateScore_ExactMatchOnOriginalTitle_ReturnsExactMatchScore()
    {
        // Arrange: torrent named "la vita e bella" matches Italian original, not English "life is beautiful"
        var torrent = new TorrentInfo { ParsedTitle = "la vita e bella", Year = 1997 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0118799",
            Title = "life is beautiful",
            OriginalTitle = "la vita e bella",
            Year = 1997,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: exact match on OriginalTitle + year
        score.Should().Be(ExactMatchTitleYearScore * 100);
    }

    [Fact]
    public void CalculateScore_CloseYearMatchOnOriginalTitle_ReturnsCloseMatchScore()
    {
        // Arrange: "Cidade de Deus" (2002) with year off by 1
        var torrent = new TorrentInfo { ParsedTitle = "cidade de deus", Year = 2003 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0317248",
            Title = "city of god",
            OriginalTitle = "cidade de deus",
            Year = 2002,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: close match on OriginalTitle (year within 1)
        score.Should().Be(CloseMatchTitleYearScore * 100);
    }

    [Fact]
    public void CalculateScore_OriginalTitleCloserFuzzyMatch_ReturnsHigherScore()
    {
        // Arrange: torrent "der untergang" is closer to German original than English "downfall"
        var torrent = new TorrentInfo { ParsedTitle = "der untergang", Year = 2020 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0363163",
            Title = "downfall",
            OriginalTitle = "der untergung",
            Year = 2004,
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
        // Arrange: torrent "life is beautiful" matches English Title better than Italian OriginalTitle
        var torrent = new TorrentInfo { ParsedTitle = "life is beautiful", Year = 1997 };
        var imdb = new ImdbFile
        {
            ImdbId = "tt0118799",
            Title = "life is beautiful",
            OriginalTitle = "la vita e bella",
            Year = 1997,
        };

        // Act
        var score = ImdbFuzzyStringMatchingService.CalculateScore(torrent, imdb);

        // Assert: should return title-based exact match score (higher than originalTitle fuzzy)
        score.Should().Be(ExactMatchTitleYearScore * 100);
    }
}
