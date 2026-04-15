using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

[DbContext(typeof(ZileanDbContext))]
[Migration("20260415120000_ImdbOriginalTitles")]
public class ImdbOriginalTitles : Migration
{
    private const string BackfillOriginalTitles =
        """
        UPDATE public."ImdbFiles"
        SET "OriginalTitle" = "Title"
        WHERE "OriginalTitle" IS NULL;
        """;

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OriginalTitle",
            table: "ImdbFiles",
            type: "text",
            nullable: true);

        migrationBuilder.Sql(BackfillOriginalTitles);
        migrationBuilder.Sql(SearchImdbProcedureV3.RemoveImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedureV4.CreateImdbProcedure);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedureV4.RemoveImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedureV3.CreateImdbProcedure);

        migrationBuilder.DropColumn(
            name: "OriginalTitle",
            table: "ImdbFiles");
    }
}
