namespace Zilean.ApiService.Features.Dmm;

public interface IDMMFileDownloader
{
    Task<string> DownloadFileToTempPath(CancellationToken cancellationToken);
}