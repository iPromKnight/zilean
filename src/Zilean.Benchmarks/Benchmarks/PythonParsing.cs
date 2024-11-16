using Zilean.Scraper.Features.PythonSupport;

namespace Zilean.Benchmarks.Benchmarks;

public class PythonParsing
{
    private ParseTorrentNameService _service = null!;
    private List<ExtractedDmmEntry>? _oneK;
    private List<ExtractedDmmEntry>? _fiveK;
    private List<ExtractedDmmEntry>? _tenK;
    private List<ExtractedDmmEntry>? _oneHundredK;

    [GlobalSetup]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "/opt/homebrew/opt/python@3.11/Frameworks/Python.framework/Versions/3.11/lib/libpython3.11.dylib");
        var logger = Substitute.For<ILogger<ParseTorrentNameService>>();
        _service = new ParseTorrentNameService(logger);
        _oneK = GenerateTorrents(1000);
        _fiveK = GenerateTorrents(5000);
        _tenK = GenerateTorrents(10000);
        _oneHundredK = GenerateTorrents(100000);
    }

    [Benchmark]
    public async Task<List<TorrentInfo>> ParseTorrent_1K_Success()
    {
        var results = await _service.ParseAndPopulateAsync(_oneK);
        return results;
    }

    [Benchmark]
    public async Task<List<TorrentInfo>> ParseTorrent_5K_Success()
    {
        var results = await _service.ParseAndPopulateAsync(_fiveK);
        return results;
    }

    [Benchmark]
    public async Task<List<TorrentInfo>> ParseTorrent_10k_Success()
    {
        var results = await _service.ParseAndPopulateAsync(_tenK);
        return results;
    }

    [Benchmark]
    public async Task<List<TorrentInfo>> ParseTorrent_100k_Success()
    {
        var results = await _service.ParseAndPopulateAsync(_oneHundredK);
        return results;
    }

    private static List<ExtractedDmmEntry> GenerateTorrents(int count)
    {
        var torrents = new List<ExtractedDmmEntry>();
        var random = new Random();
        var titles = new[]
        {
            "Iron.Man.2008.INTERNAL.REMASTERED.2160p.UHD.BluRay.X265-IAMABLE",
            "Harry.Potter.and.the.Sorcerers.Stone.2001.2160p.UHD.BluRay.X265-IAMABLE",
            "The.Dark.Knight.2008.2160p.UHD.BluRay.X265-IAMABLE",
            "Inception.2010.2160p.UHD.BluRay.X265-IAMABLE",
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
}
