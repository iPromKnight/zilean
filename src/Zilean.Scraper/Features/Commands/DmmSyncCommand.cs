namespace Zilean.Scraper.Features.Commands;

public class DmmSyncCommand(DmmScraping dmmScraping) : Command
{
    public override int Execute(CommandContext context) =>
        dmmScraping.Execute(CancellationToken.None).GetAwaiter().GetResult();
}
