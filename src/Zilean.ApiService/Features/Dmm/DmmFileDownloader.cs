namespace Zilean.ApiService.Features.Dmm;

public interface IDmmFileDownloader
{
    Task<string> DownloadFileToTempPath(CancellationToken cancellationToken);
}

public class DmmFileDownloader(HttpClient client, ILogger<DmmFileDownloader> logger) : IDmmFileDownloader
{
    private const string Filename = "main.zip";

    private readonly IReadOnlyCollection<string> _filesToIgnore =
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

        var response = await client.GetAsync(Filename, cancellationToken);

        var tempDirectory = Path.Combine(Path.GetTempPath(), "DMMHashlists");

        EnsureDirectoryIsClean(tempDirectory);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Save the stream to a temporary file
        var tempFilePath = Path.Combine(Path.GetTempPath(), "DMMHashlists.zip");
        await SaveStreamToFile(stream, tempFilePath, cancellationToken);

        // Extract the temporary file
        ExtractZipFile(tempFilePath, tempDirectory);

        // Delete the temporary file
        File.Delete(tempFilePath);

        foreach (var file in _filesToIgnore)
        {
            CleanRepoExtras(tempDirectory, file);
        }

        logger.LogInformation("Downloaded and extracted Repository to {TempDirectory}", tempDirectory);

        return tempDirectory;
    }

    private static async Task SaveStreamToFile(Stream stream, string filePath, CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    private static void ExtractZipFile(string zipFilePath, string extractPath)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);

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
