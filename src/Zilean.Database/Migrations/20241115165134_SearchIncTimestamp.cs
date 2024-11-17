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
        migrationBuilder.Sql(SearchTorrentsMetaV2.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV3.Create);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV3.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV2.Create);
    }
}
