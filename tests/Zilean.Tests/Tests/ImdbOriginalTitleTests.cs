using Zilean.Database.Services;

namespace Zilean.Tests.Tests;

public class ImdbOriginalTitleTests
{
    [Fact]
    public void calculate_score_uses_original_title_when_primary_title_does_not_match()
    {
        var torrent = new TorrentInfo
        {
            ParsedTitle = "Le Fabuleux Destin d'Amelie Poulain",
            Year = 2001,
            Category = "movie"
        };

        var imdb = new ImdbFile
        {
            ImdbId = "tt0211915",
            Title = "Amelie",
            OriginalTitle = "Le Fabuleux Destin d'Amelie Poulain",
            Year = 2001,
            Category = "movie"
        };

        var score = ImdbTitleMatching.CalculateScore(torrent, imdb);

        score.Should().Be(200);
    }

    [Fact]
    public async Task import_reads_original_title_from_imdb_basics()
    {
        var logger = Substitute.For<ILogger<ImdbFileProcessor>>();
        var imdbFileService = Substitute.For<IImdbFileService>();
        var processor = new ImdbFileProcessor(logger, imdbFileService);
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tsv");

        try
        {
            await File.WriteAllTextAsync(
                filePath,
                """
                tconst	titleType	primaryTitle	originalTitle	isAdult	startYear	endYear	runtimeMinutes	genres
                tt0211915	movie	Amelie	Le Fabuleux Destin d'Amelie Poulain	0	2001	\N	122	Comedy,Romance
                """);

            await processor.Import(filePath, CancellationToken.None);

            imdbFileService.Received(1).AddImdbFile(Arg.Is<ImdbFile>(file =>
                file.ImdbId == "tt0211915" &&
                file.Title == "Amelie" &&
                file.OriginalTitle == "Le Fabuleux Destin d'Amelie Poulain" &&
                file.Year == 2001 &&
                file.Category == "movie"));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
