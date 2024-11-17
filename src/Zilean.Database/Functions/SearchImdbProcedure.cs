namespace Zilean.Database.Functions;

public static class SearchImdbProcedure
{
    internal const string CreateImdbProcedure =
        """
            CREATE OR REPLACE FUNCTION search_imdb_meta(search_term TEXT, category_param TEXT DEFAULT NULL, year_param INT DEFAULT NULL, limit_param INT DEFAULT 10, similarity_threshold REAL DEFAULT 0.85)
        RETURNS TABLE(imdb_id text, title text, category text, year INT, score REAL) AS $$
            BEGIN
        EXECUTE format('SET pg_trgm.similarity_threshold = %L', similarity_threshold);
        RETURN QUERY
            SELECT "ImdbId", "Title", "Category", "Year", similarity("Title", search_term) as score
            FROM public."ImdbFiles"
            WHERE ("Title" % search_term)
              AND ("Adult" = FALSE)
              AND (category_param IS NULL OR "Category" = category_param)
              AND (year_param IS NULL OR "Year" BETWEEN year_param - 1 AND year_param + 1)
            ORDER BY score DESC
            LIMIT limit_param;
            END; $$
        LANGUAGE plpgsql;
        """;

    internal const string RemoveImdbProcedure = "DROP FUNCTION IF EXISTS search_imdb_meta(TEXT, TEXT, INT, INT);";
}
