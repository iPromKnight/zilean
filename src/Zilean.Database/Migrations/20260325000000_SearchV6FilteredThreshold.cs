using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class SearchV6FilteredThreshold : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV5.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV6.Create);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV6.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV5.Create);
    }
}
