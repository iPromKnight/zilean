using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class EnableUnaccent : Migration
{
    private const string EnableUnaccentExtension = "CREATE EXTENSION IF NOT EXISTS unaccent;";
    private const string DisableUnaccentExtension = "DROP EXTENSION IF EXISTS unaccent;";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(EnableUnaccentExtension);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(DisableUnaccentExtension);
}
