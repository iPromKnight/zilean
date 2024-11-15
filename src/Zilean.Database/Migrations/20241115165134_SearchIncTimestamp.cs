using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class SearchIncTimestamp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedureV3);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedureV2);
    }
}
