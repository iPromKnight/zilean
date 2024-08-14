namespace Zilean.Database.Functions;

internal static class TorrentsFetchImdbId
{
    internal const string CreateProcedure =
        """
        CREATE OR REPLACE FUNCTION torrents_fetch_imdb_id()
        RETURNS TRIGGER AS $$
        DECLARE
            imdb_record RECORD;
        BEGIN
            SELECT INTO imdb_record *
            FROM search_imdb_meta(NEW."Title", NEW."Category", NEW."Year", 1, 0.85)
            LIMIT 1;

            IF imdb_record.imdb_id IS NOT NULL THEN
                NEW."ImdbId" := imdb_record.imdb_id;
            ELSE
                RAISE NOTICE 'No matching IMDb record found for title: %, category: %, year: %', NEW."Title", NEW."Category", NEW."Year";
            END IF;

            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
        """;

    internal const string RemoveProcedure = "DROP FUNCTION IF EXISTS torrents_fetch_imdb_id();";
}
