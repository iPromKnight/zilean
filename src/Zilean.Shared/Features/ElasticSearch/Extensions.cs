namespace Zilean.Shared.Features.ElasticSearch;

public static class Extensions
{
    public static ConnectionSettings SetupDebugMode(this ConnectionSettings settings)
    {
        settings.DisableDirectStreaming();
        settings.PrettyJson();
        settings.EnableDebugMode(details =>
        {
            Console.WriteLine(details.DebugInformation);
        });

        return settings;
    }
}
