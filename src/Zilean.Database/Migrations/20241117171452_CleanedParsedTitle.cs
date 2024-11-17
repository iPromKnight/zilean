using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class CleanedParsedTitle : Migration
{
    private const string UpdateTorrentsCleanedParsedTitle =
        """
        UPDATE "Torrents"
        SET "CleanedParsedTitle" = regexp_replace(
            regexp_replace(
                "ParsedTitle",
                '(^|\s)(?:a|the|and|of|in|on|with|to|for|by|is|it)(?=\s|$)',
                '\1',
                'gi'
            ),
            '^\s+|\s{2,}',
            '',
            'g'
        )
        WHERE "ParsedTitle" IS NOT NULL;
        """;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CleanedParsedTitle",
            table: "Torrents",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(SearchTorrentsMetaV3.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV4.Create);
        migrationBuilder.Sql(UpdateTorrentsCleanedParsedTitle);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CleanedParsedTitle",
            table: "Torrents");

        migrationBuilder.Sql(SearchTorrentsMetaV4.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV3.Create);
    }
}
