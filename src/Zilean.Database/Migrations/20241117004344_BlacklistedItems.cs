using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class BlacklistedItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BlacklistedItems",
            columns: table => new
            {
                InfoHash = table.Column<string>(type: "text", nullable: false),
                Reason = table.Column<string>(type: "text", nullable: false),
                BlacklistedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BlacklistedItems", x => x.InfoHash);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BlacklistedItems_InfoHash",
            table: "BlacklistedItems",
            column: "InfoHash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BlacklistedItems");
    }
}
