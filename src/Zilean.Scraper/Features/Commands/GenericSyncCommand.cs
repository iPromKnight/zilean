namespace Zilean.Scraper.Features.Commands;

public class GenericSyncCommand(GenericIngestionScraping genericIngestion) : AsyncCommand
{
    public override Task<int> ExecuteAsync(CommandContext context) =>
        genericIngestion.Execute(CancellationToken.None);
}
