using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class SearchImdbV2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedureV2.CreateImdbProcedure);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedureV2.RemoveImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateImdbProcedure);
    }
}
