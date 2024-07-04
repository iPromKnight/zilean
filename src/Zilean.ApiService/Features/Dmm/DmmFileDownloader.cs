namespace Zilean.ApiService.Features.Dmm;

public interface IDmmFileDownloader
{
    Task<string> DownloadFileToTempPath(CancellationToken cancellationToken);
}

public class DmmFileDownloader(HttpClient client, ILogger<DmmFileDownloader> logger) : IDmmFileDownloader
{
    private const string Filename = "main.zip";

    private static readonly IReadOnlyCollection<string> _filesToIgnore =
    [
        "index.html",
        "404.html",
        "dedupe.sh",
        "CNAME",
    ];

    public const string ClientName = "DmmFileDownloader";

    public async Task<string> DownloadFileToTempPath(CancellationToken cancellationToken)
    {
        logger.LogInformation("Downloading DMM Hashlists");

        var response = await client.GetAsync(Filename, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var tempDirectory = Path.Combine(Path.GetTempPath(), "DMMHashlists");

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
}
