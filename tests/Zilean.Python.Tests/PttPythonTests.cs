using Zilean.DmmScraper.Features.Python;
using Zilean.Shared.Features.Dmm;

namespace Zilean.Python.Tests;

public class PttPythonTests
{
    public PttPythonTests() =>
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "/opt/homebrew/opt/python@3.11/Frameworks/Python.framework/Versions/3.11/lib/libpython3.11.dylib");

    [Fact]
    public async Task EnsureInitAndStop_Success()
    {
        var logger = Substitute.For<ILogger<ParseTorrentNameService>>();
        var service = new ParseTorrentNameService(logger);

        await service.StopPythonEngine();
    }

    [Fact]
    public async Task ParseTorrent_Success()
    {
        var logger = Substitute.For<ILogger<ParseTorrentNameService>>();
        var service = new ParseTorrentNameService(logger);

        var torrents = GenerateTorrents(3000);

        var result = await service.ParseAndPopulateAsync(torrents);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        foreach (var response in result)
        {
            Assert.NotNull(response);
        }
    }

    public static List<ExtractedDmmEntry> GenerateTorrents(int count)
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
