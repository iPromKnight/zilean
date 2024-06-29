namespace Zilean.ApiService.Features.Dmm;

public interface IDebridMediaManagerCrawler
{
    Task Execute(CancellationToken cancellationToken);
    bool IsRunning { get; }
}
