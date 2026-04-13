namespace Zilean.Tests.Tests.Imdb;

public class ImdbFileProcessorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IImdbFileService _mockImdbFileService;
    private readonly ImdbFileProcessor _processor;
    private readonly List<ImdbFile> _capturedFiles;

    public ImdbFileProcessorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _capturedFiles = [];
        _mockImdbFileService = Substitute.For<IImdbFileService>();
        _mockImdbFileService
            .When(x => x.AddImdbFile(Arg.Any<ImdbFile>()))
            .Do(callInfo => _capturedFiles.Add(callInfo.Arg<ImdbFile>()));
        _mockImdbFileService.StoreImdbFiles().Returns(Task.CompletedTask);
        _mockImdbFileService.VaccumImdbFilesIndexes(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var logger = Substitute.For<ILogger<ImdbFileProcessor>>();
        _processor = new ImdbFileProcessor(logger, _mockImdbFileService);
    }

    private string CreateTsvFile(params string[] dataRows)
    {
        // The processor has HasHeaderRecord=true (CsvHelper auto-reads the header)
        // PLUS a manual csv.ReadAsync() that skips the next row.
        // So we need a sacrificial first data row that gets consumed by the manual skip.
        var header = "tconst\ttitleType\tprimaryTitle\toriginalTitle\tisAdult\tstartYear\tendYear\truntimeMinutes\tgenres";
        var sacrificialRow = "tt0000000\tmovie\tSacrificial\tSacrificial\t0\t2000\t\\N\t90\tDrama";
        var lines = new List<string> { header, sacrificialRow };
        lines.AddRange(dataRows);

        var filePath = Path.Combine(_tempDir, "title.basics.tsv");
        File.WriteAllLines(filePath, lines);
        return filePath;
    }

    [Fact]
    public async Task Parser_ReadsOriginalTitle_WhenDifferentFromPrimaryTitle()
    {
        // Arrange: "La vita è bella" (Italian) vs "Life Is Beautiful" (English) — completely different strings
        var filePath = CreateTsvFile(
            "tt0118799\tmovie\tLife Is Beautiful\tLa vita è bella\t0\t1997\t\\N\t116\tComedy,Drama,Romance"
        );

        // Act
        await _processor.Import(filePath, CancellationToken.None);

        // Assert
        _capturedFiles.Should().HaveCount(1);
        _capturedFiles[0].OriginalTitle.Should().Be("La vita è bella");
        _capturedFiles[0].Title.Should().Be("Life Is Beautiful");
        _capturedFiles[0].ImdbId.Should().Be("tt0118799");
    }

    [Fact]
    public async Task Parser_ReadsOriginalTitle_WhenSameAsPrimaryTitle()
    {
        // Arrange: Breaking Bad has the same original and primary title
        var filePath = CreateTsvFile(
            "tt0903747\ttvSeries\tBreaking Bad\tBreaking Bad\t0\t2008\t2013\t49\tCrime,Drama,Thriller"
        );

        // Act
        await _processor.Import(filePath, CancellationToken.None);

        // Assert
        _capturedFiles.Should().HaveCount(1);
        _capturedFiles[0].OriginalTitle.Should().Be("Breaking Bad");
        _capturedFiles[0].Title.Should().Be("Breaking Bad");
    }

    [Fact]
    public async Task Parser_SetsOriginalTitleToNull_WhenFieldIsBackslashN()
    {
        // Arrange: \N sentinel in originalTitle column should become null
        var filePath = CreateTsvFile(
            "tt9999999\tmovie\tTest Movie\t\\N\t0\t2020\t\\N\t90\tDrama"
        );

        // Act
        await _processor.Import(filePath, CancellationToken.None);

        // Assert
        _capturedFiles.Should().HaveCount(1);
        _capturedFiles[0].OriginalTitle.Should().BeNull();
        _capturedFiles[0].Title.Should().Be("Test Movie");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}
