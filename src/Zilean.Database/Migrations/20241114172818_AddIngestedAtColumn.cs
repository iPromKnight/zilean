using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class AddIngestedAtColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<DateTime>(
            name: "IngestedAt",
            table: "Torrents",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "now() at time zone 'utc'");

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "IngestedAt",
            table: "Torrents");
}
