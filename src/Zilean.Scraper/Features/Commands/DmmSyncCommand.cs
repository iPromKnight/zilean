namespace Zilean.Scraper.Features.Commands;

public class DmmSyncCommand(DmmScraping dmmScraping) : AsyncCommand
{
    public override Task<int> ExecuteAsync(CommandContext context) =>
        dmmScraping.Execute(CancellationToken.None);
}
