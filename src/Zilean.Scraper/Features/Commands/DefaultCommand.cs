namespace Zilean.Scraper.Features.Commands;

public sealed class DefaultCommand(ILogger<DefaultCommand> logger) : Command<DefaultCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        logger.LogInformation("Zilean Scraper: Execution Completed");
        return 0;
    }
}
