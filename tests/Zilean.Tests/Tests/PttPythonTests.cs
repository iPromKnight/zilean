using Zilean.Shared.Features.Python;

namespace Zilean.Tests.Tests;

public class PttPythonTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ParseTorrentNameService _parseTorrentNameService;

    public PttPythonTests(ITestOutputHelper output)
    {
        _output = output;
        Environment.SetEnvironmentVariable("ZILEAN_PYTHON_PYLIB",
            "/opt/homebrew/opt/python@3.11/Frameworks/Python.framework/Versions/3.11/lib/libpython3.11.dylib");

        var loggerParse = Substitute.For<ILogger<ParseTorrentNameService>>();
        _parseTorrentNameService = new ParseTorrentNameService(loggerParse);
    }

    [Fact]
    public async Task ParseTorrent_Movie_Success()
    {
        var stopwatch = new Stopwatch();

        var torrents = GenerateMovieTorrents(20);

        stopwatch.Start();
        var result = await _parseTorrentNameService.ParseAndPopulateAsync(torrents);
        var elapsed = stopwatch.Elapsed;
        _output.WriteLine($"Parsed {20} torrents in {elapsed.TotalSeconds} seconds");

        result.Should().NotBeNull().And.NotBeEmpty();
        result.Should().AllBeOfType<TorrentInfo>();
    }

    [Fact]
    public async Task ParseTorrent_TvSeries_Success()
    {
        var stopwatch = new Stopwatch();

        var torrents = GenerateTvTorrents(20);

        stopwatch.Start();
        var result = await _parseTorrentNameService.ParseAndPopulateAsync(torrents);
        var elapsed = stopwatch.Elapsed;
        _output.WriteLine($"Parsed {20} torrents in {elapsed.TotalSeconds} seconds");

        result.Should().NotBeNull().And.NotBeEmpty();
        result.Should().AllBeOfType<TorrentInfo>();
    }

    private static List<ExtractedDmmEntry> GenerateMovieTorrents(int count)
    {
        var torrents = new List<ExtractedDmmEntry>();
        var random = new Random();
        var titles = new[]
        {
            "Iron.Man.2008.INTERNAL.REMASTERED.2160p.UHD.BluRay.X265-IAMABLE",
            "Harry.Potter.and.the.Sorcerers.Stone.2001.2160p.UHD.BluRay.X265-IAMABLE",
            "The.Dark.Knight.2008.2160p.UHD.BluRay.X265-IAMABLE", "Inception.2010.2160p.UHD.BluRay.X265-IAMABLE",
            "The.Matrix.1999.2160p.UHD.BluRay.X265-IAMABLE"
        };

        for (int i = 0; i < count; i++)
        {
            var infoHash = $"1234562828797{i:D4}";
            var filename = titles[random.Next(titles.Length)];
            var filesize = (long)(random.NextDouble() * 100000000000);

            torrents.Add(new ExtractedDmmEntry(infoHash, filename, filesize, null));
        }

        return torrents;
    }

    private static List<ExtractedDmmEntry> GenerateTvTorrents(int count)
    {
        var torrents = new List<ExtractedDmmEntry>();
        var random = new Random();

        var titles = new[]
        {
            "The.Witcher.S01E01.1080p.WEB.H264-METCON", "The.Witcher.S01E02.1080p.WEB.H264-METCON",
            "The.Witcher.S01E03.1080p.WEB.H264-METCON", "The.Witcher.S01E04.1080p.WEB.H264-METCON",
            "The.Witcher.S01E05.1080p.WEB.H264-METCON",
        };

        for (int i = 0; i < count; i++)
        {
            var infoHash = $"1234562828797{i:D4}";
            var filename = titles[random.Next(titles.Length)];
            var filesize = (long)(random.NextDouble() * 100000000000);

            torrents.Add(new ExtractedDmmEntry(infoHash, filename, filesize, null));
        }

        return torrents;
    }

    public void Dispose() => _parseTorrentNameService.StopPythonEngine().GetAwaiter().GetResult();
}
