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
            "Resolution" TEXT,
            "Year" INT,
            "Remastered" BOOLEAN,
            "Codec" TEXT,
            "Audio" TEXT[],
            "Quality" TEXT,
            "Episodes" INT[],
            "Seasons" INT[],
            "Languages" TEXT[],
            "ParsedTitle" TEXT,
            "NormalizedTitle" TEXT,
            "RawTitle" TEXT,
            "Size" TEXT,
            "Category" TEXT,
            "Complete" BOOLEAN,
            "Volumes" INT[],
            "Hdr" TEXT[],
            "Channels" TEXT[],
            "Dubbed" BOOLEAN,
            "Subbed" BOOLEAN,
            "Edition" TEXT,
            "BitDepth" TEXT,
            "Bitrate" TEXT,
            "Network" TEXT,
            "Extended" BOOLEAN,
            "Converted" BOOLEAN,
            "Hardcoded" BOOLEAN,
            "Region" TEXT,
            "Ppv" BOOLEAN,
            "Is3d" BOOLEAN,
            "Site" TEXT,
            "Proper" BOOLEAN,
            "Repack" BOOLEAN,
            "Retail" BOOLEAN,
            "Upscaled" BOOLEAN,
            "Unrated" BOOLEAN,
            "Documentary" BOOLEAN,
            "EpisodeCode" TEXT,
            "Country" TEXT,
            "Container" TEXT,
            "Extension" TEXT,
            "Torrent" BOOLEAN,
            "ImdbId" TEXT,
            "ImdbCategory" TEXT,
            "ImdbTitle" TEXT,
            "ImdbYear" INT,
            "ImdbAdult" BOOLEAN
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
                t."ParsedTitle",
                t."NormalizedTitle",
                t."RawTitle",
                t."Size",
                t."Category",
                t."Complete",
                t."Volumes",
                t."Hdr",
                t."Channels",
                t."Dubbed",
                t."Subbed",
                t."Edition",
                t."BitDepth",
                t."Bitrate",
                t."Network",
                t."Extended",
                t."Converted",
                t."Hardcoded",
                t."Region",
                t."Ppv",
                t."Is3d",
                t."Site",
                t."Proper",
                t."Repack",
                t."Retail",
                t."Upscaled",
                t."Unrated",
                t."Documentary",
                t."EpisodeCode",
                t."Country",
                t."Container",
                t."Extension",
                t."Torrent",
                t."ImdbId",
                similarity(t."ParsedTitle", query) AS "Score",
                i."Category" AS "ImdbCategory",
                i."Title" AS "ImdbTitle",
                i."Year" AS "ImdbYear",
                i."Adult" AS "ImdbAdult"
            FROM
                public."Torrents" t
            LEFT JOIN
                public."ImdbFiles" i ON t."ImdbId" = i."ImdbId"
            WHERE
                (query IS NULL OR t."ParsedTitle" % query)
            AND (season IS NULL OR season = ANY(t."Seasons"))
            AND (
                (episode IS NULL AND season IS NOT NULL)
                OR
                (
                    episode IS NOT NULL AND
                    season IS NOT NULL AND
                    (episode = ANY(t."Episodes") OR t."Episodes" IS NULL OR t."Episodes" = '{}')
                )
                OR (season IS NULL AND episode IS NULL)
            )
            AND (year IS NULL OR t."Year" BETWEEN year - 1 AND year + 1)
            AND (language IS NULL OR language = ANY(t."Languages"))
            AND (resolution IS NULL OR resolution = t."Resolution")
            AND (imdbId IS NULL OR t."ImdbId" = imdbId)
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
