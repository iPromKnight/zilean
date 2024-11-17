namespace Zilean.Scraper.Features.Commands;

public class GenericSyncCommand(GenericIngestionScraping genericIngestion) : Command
{
    public override int Execute(CommandContext context) =>
        genericIngestion.Execute(CancellationToken.None).GetAwaiter().GetResult();
}
