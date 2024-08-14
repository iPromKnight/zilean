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

    internal const string CreateTorrentProcedure =
        """
        CREATE OR REPLACE FUNCTION search_torrents_meta(
            query TEXT DEFAULT NULL,
            season INT DEFAULT NULL,
            episode INT DEFAULT NULL,
            year INT DEFAULT NULL,
            language TEXT DEFAULT NULL,
            resolution TEXT DEFAULT NULL,
            imdbId TEXT DEFAULT NULL,
            limit_param INT DEFAULT 20,
            similarity_threshold REAL DEFAULT 0.85
        )
        RETURNS TABLE(
            "InfoHash" TEXT,
            "Resolution" TEXT[],
            "Year" INT,
            "Remastered" BOOLEAN,
            "Codec" TEXT[],
            "Audio" TEXT[],
            "Quality" TEXT[],
            "Episodes" INT[],
            "Seasons" INT[],
            "Languages" TEXT[],
            "Title" TEXT,
            "RawTitle" TEXT,
            "Size" BIGINT,
            "Category" TEXT,
            "Score" REAL,
            "ImdbId" TEXT,
            "ImdbCategory" TEXT, -- Renamed to avoid conflict
            "ImdbTitle" TEXT,    -- Renamed to avoid conflict
            "ImdbYear" INT,      -- Renamed to avoid conflict
            "ImdbAdult" BOOLEAN  -- Renamed to avoid conflict
        ) AS $$
        BEGIN
            EXECUTE format('SET pg_trgm.similarity_threshold = %L', similarity_threshold);

            RETURN QUERY
            SELECT
                t."InfoHash",
                t."Resolution",
                t."Year",
                t."Remastered",
                t."Codec",
                t."Audio",
                t."Quality",
                t."Episodes",
                t."Seasons",
                t."Languages",
                t."Title",
                t."RawTitle",
                t."Size",
                t."Category",
                similarity(t."Title", query) AS "Score",
                t."ImdbId",
                i."Category" AS "ImdbCategory",  -- Aliased to avoid conflict
                i."Title" AS "ImdbTitle",        -- Aliased to avoid conflict
                i."Year" AS "ImdbYear",          -- Aliased to avoid conflict
                i."Adult" AS "ImdbAdult"         -- Aliased to avoid conflict
            FROM
                public."Torrents" t
            LEFT JOIN
                public."ImdbFiles" i ON t."ImdbId" = i."ImdbId"
            WHERE
                (query IS NULL OR t."Title" % query)
                AND (season IS NULL OR season = ANY(t."Seasons"))
                AND (episode IS NULL OR episode = ANY(t."Episodes"))
                AND (year IS NULL OR t."Year" BETWEEN year - 1 AND year + 1)
                AND (language IS NULL OR language = ANY(t."Languages"))
                AND (resolution IS NULL OR resolution = ANY(t."Resolution"))
                AND (imdbId IS NULL OR imdbId = t."ImdbId")
            ORDER BY
                "Score" DESC
            LIMIT
                limit_param;
        END;
        $$ LANGUAGE plpgsql;
        """;

    internal const string RemoveImdbProcedure = "DROP FUNCTION IF EXISTS search_imdb_meta(TEXT, TEXT, INT, INT);";
    internal const string RemoveTorrentProcedure = "DROP FUNCTION IF EXISTS search_torrents_meta(TEXT, INT, INT, INT, TEXT, TEXT, TEXT, INT, REAL);";
}
