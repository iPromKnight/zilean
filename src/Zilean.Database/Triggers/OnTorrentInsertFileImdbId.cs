namespace Zilean.Database.Triggers;

internal static class OnTorrentInsertFileImdbId
{
    internal const string CreateTrigger =
        """
        CREATE TRIGGER on_torrent_find_imdb_id
        BEFORE INSERT ON public."Torrents"
        FOR EACH ROW
        EXECUTE FUNCTION torrents_fetch_imdb_id();
        """;

    internal const string RemoveTrigger = "DROP TRIGGER IF EXISTS on_torrent_find_imdb_id ON public.\"Torrents\";";
}
