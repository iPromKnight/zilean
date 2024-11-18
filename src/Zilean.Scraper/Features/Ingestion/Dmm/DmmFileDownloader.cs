namespace Zilean.Scraper.Features.Ingestion.Dmm;

public class DmmFileDownloader(ILogger<DmmFileDownloader> logger, ZileanConfiguration configuration)
{
    private const string Filename = "main.zip";

    private static readonly IReadOnlyCollection<string> _filesToIgnore =
    [
        "index.html",
        "404.html",
        "dedupe.sh",
        "CNAME",
    ];

    public async Task<string> DownloadFileToTempPath(DmmLastImport? dmmLastImport, CancellationToken cancellationToken)
    {
        logger.LogInformation("Downloading DMM Hashlists");

        var tempDirectory = Path.Combine(Path.GetTempPath(), "DMMHashlists");

        if (dmmLastImport is not null)
        {
            if (DateTime.UtcNow - dmmLastImport.OccuredAt < TimeSpan.FromMinutes(configuration.Dmm.MinimumReDownloadIntervalMinutes))
            {
                logger.LogInformation("DMM Hashlists download not required as last download was less than the configured {Minutes} minutes re-download interval set in DMM Configuration.", configuration.Dmm.MinimumReDownloadIntervalMinutes);
                return tempDirectory;
            }
        }

        var client = CreateHttpClient();
        var response = await client.GetAsync(Filename, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        EnsureDirectoryIsClean(tempDirectory);

        response.EnsureSuccessStatusCode();

        var tempFilePath = Path.Combine(tempDirectory, "DMMHashlists.zip");
        await using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
        await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        ExtractZipFile(tempFilePath, tempDirectory);

        File.Delete(tempFilePath);

        foreach (var file in _filesToIgnore)
        {
            CleanRepoExtras(tempDirectory, file);
        }

        logger.LogInformation("Downloaded and extracted Repository to {TempDirectory}", tempDirectory);

        return tempDirectory;
    }

    private static void ExtractZipFile(string zipFilePath, string extractPath)
    {
        using var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            var entryPath = Path.Combine(extractPath, Path.GetFileName(entry.FullName));
            if (!entry.FullName.EndsWith('/'))
            {
                entry.ExtractToFile(entryPath, true);
            }
        }
    }

    private static void CleanRepoExtras(string tempDirectory, string fileName)
    {
        var repoIndex = Path.Combine(tempDirectory, fileName);

        if (File.Exists(repoIndex))
        {
            File.Delete(repoIndex);
        }
    }

    private static void EnsureDirectoryIsClean(string tempDirectory)
    {
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }

        Directory.CreateDirectory(tempDirectory);
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://github.com/debridmediamanager/hashlists/zipball/main/"),
            Timeout = TimeSpan.FromMinutes(10),
        };

        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("curl/7.54");
        return httpClient;
    }
}
