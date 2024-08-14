namespace Zilean.DmmScraper.Features.Imdb;

public class ImdbFileDownloader(ILogger<ImdbFileDownloader> logger)
{
    private static readonly string _dataFilePath = Path.Combine(AppContext.BaseDirectory, "data", TitleBasicsFileName);
    private const string TitleBasicsFileName = "title.basics.tsv";
    private const string ImdbDataBaseAddress = "https://datasets.imdbws.com/";

    public async Task<string> DownloadMetadataFile(CancellationToken cancellationToken) =>
        await DownloadFileToTempPath(TitleBasicsFileName, cancellationToken);

    private async Task<string> DownloadFileToTempPath(string fileName, CancellationToken cancellationToken)
    {
        if (File.Exists(_dataFilePath))
        {
            var fileInfo = new FileInfo(_dataFilePath);
            if (fileInfo.CreationTimeUtc <= DateTime.UtcNow.AddDays(30))
            {
                logger.LogInformation("IMDB data '{Filename}' already exists at {TempFile}. Will use records in that for import.", fileName, _dataFilePath);
                return _dataFilePath;
            }

            logger.LogInformation("IMDB data '{Filename}' is older than 30 days, deleting", fileName);
            File.Delete(_dataFilePath);
        }

        logger.LogInformation("Downloading IMDB data '{Filename}'", fileName);

        var client = CreateHttpClient();
        var response = await client.GetAsync($"{fileName}.gz", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        await using var fileStream = File.Create(_dataFilePath);

        await gzipStream.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Downloaded IMDB data '{Filename}' to {TempFile}", fileName, _dataFilePath);

        fileStream.Close();
        return _dataFilePath;
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
