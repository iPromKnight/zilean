using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class CleanedParsedTitleIndexes : Migration
{
    private const string AddBIndexSupport =
        """
        CREATE EXTENSION IF NOT EXISTS btree_gin;
        CREATE EXTENSION IF NOT EXISTS btree_gist;
        """;

    private const string RemoveBIndexSupport =
        """
        DROP EXTENSION IF EXISTS btree_gin;
        DROP EXTENSION IF EXISTS btree_gist;
        """;

    private const string CreateIndexes =
        """
        CREATE INDEX idx_infohash_length_40 ON "Torrents" (length("InfoHash"));
        """;

    private const string RemoveIndexes = "DROP INDEX IF EXISTS idx_infohash_length_40;";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(AddBIndexSupport);

        migrationBuilder.Sql(CreateIndexes);

        migrationBuilder.RenameIndex(
            name: "IX_Torrents_ImdbId",
            table: "Torrents",
            newName: "idx_torrents_imdbid");

        migrationBuilder.CreateIndex(
            name: "idx_cleaned_parsed_title_trgm",
            table: "Torrents",
            column: "CleanedParsedTitle")
            .Annotation("Npgsql:IndexMethod", "GIN")
            .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

        migrationBuilder.CreateIndex(
            name: "idx_episodes_gin",
            table: "Torrents",
            column: "Episodes")
            .Annotation("Npgsql:IndexMethod", "GIN");

        migrationBuilder.CreateIndex(
            name: "idx_ingested_at",
            table: "Torrents",
            column: "IngestedAt",
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "idx_languages_gin",
            table: "Torrents",
            column: "Languages")
            .Annotation("Npgsql:IndexMethod", "GIN");

        migrationBuilder.CreateIndex(
            name: "idx_seasons_gin",
            table: "Torrents",
            column: "Seasons")
            .Annotation("Npgsql:IndexMethod", "GIN");

        migrationBuilder.CreateIndex(
            name: "idx_year",
            table: "Torrents",
            column: "Year");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_cleaned_parsed_title_trgm",
            table: "Torrents");

        migrationBuilder.DropIndex(
            name: "idx_episodes_gin",
            table: "Torrents");

        migrationBuilder.DropIndex(
            name: "idx_ingested_at",
            table: "Torrents");

        migrationBuilder.DropIndex(
            name: "idx_languages_gin",
            table: "Torrents");

        migrationBuilder.DropIndex(
            name: "idx_seasons_gin",
            table: "Torrents");

        migrationBuilder.DropIndex(
            name: "idx_year",
            table: "Torrents");

        migrationBuilder.RenameIndex(
            name: "idx_torrents_imdbid",
            table: "Torrents",
            newName: "IX_Torrents_ImdbId");

        migrationBuilder.Sql(RemoveIndexes);

        migrationBuilder.Sql(RemoveBIndexSupport);
    }
}
