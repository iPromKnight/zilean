namespace Zilean.ImdbLoader.Features.Imdb;

public class FileDownloader(ILogger<FileDownloader> logger)
{
    private const string TitleBasicsFileName = "title.basics.tsv";
    private const string ImdbDataBaseAddress = "https://datasets.imdbws.com/";

    public async Task<string> DownloadMetadataFile(CancellationToken cancellationToken) =>
        await DownloadFileToTempPath(TitleBasicsFileName, cancellationToken);

    public void DeleteMetadataFile(string file)
    {
        if (!File.Exists(file))
        {
            return;
        }

        File.Delete(file);
        logger.LogInformation("Deleted IMDB data file {File}", file);
    }

    private async Task<string> DownloadFileToTempPath(string fileName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Downloading IMDB data '{Filename}'", fileName);

        var client = CreateHttpClient();
        var response = await client.GetAsync($"{fileName}.gz", cancellationToken);

        var tempFile = Path.Combine(Path.GetTempPath(), fileName);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        await using var fileStream = File.Create(tempFile);

        await gzipStream.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Downloaded IMDB data '{Filename}' to {TempFile}", fileName, tempFile);

        fileStream.Close();
        return tempFile;
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ImdbDataBaseAddress),
            Timeout = TimeSpan.FromMinutes(30),
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("curl/7.54");
        return httpClient;
    }
}
