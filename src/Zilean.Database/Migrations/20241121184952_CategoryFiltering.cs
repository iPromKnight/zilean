using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class CategoryFiltering : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV4.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV5.Create);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV5.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV4.Create);
    }
}
