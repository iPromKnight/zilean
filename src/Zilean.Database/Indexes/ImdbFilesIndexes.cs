namespace Zilean.Database.Indexes;

public static class ImdbFilesIndexes
{
    internal const string CreateIndexes =
        """
        CREATE INDEX idx_imdb_metadata_adult ON public."ImdbFiles"("Adult");
        CREATE INDEX idx_imdb_metadata_category ON public."ImdbFiles"("Category");
        CREATE INDEX idx_imdb_metadata_year ON public."ImdbFiles"("Year");
        CREATE INDEX title_gin ON public."ImdbFiles" USING gin("Title" gin_trgm_ops);
        CREATE INDEX torrents_title_gin ON public."Torrents" USING gin("Title" gin_trgm_ops);
        """;

    internal const string RemoveIndexes =
        """
        DROP INDEX IF EXISTS idx_imdb_metadata_adult;
        DROP INDEX IF EXISTS idx_imdb_metadata_category;
        DROP INDEX IF EXISTS idx_imdb_metadata_year;
        DROP INDEX IF EXISTS title_gin;
        DROP INDEX IF EXISTS torrents_title_gin;
        """;

}
