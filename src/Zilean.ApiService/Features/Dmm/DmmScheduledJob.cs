namespace Zilean.ApiService.Features.Dmm;

public class DmmScheduledJob(IDebridMediaManagerCrawler crawler) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    public Task Invoke() => crawler.Execute(CancellationToken);
}
