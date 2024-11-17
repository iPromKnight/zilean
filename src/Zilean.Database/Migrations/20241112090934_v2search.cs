using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class v2search : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedureV2);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedure);
    }
}
