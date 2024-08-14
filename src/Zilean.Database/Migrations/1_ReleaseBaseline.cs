using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class ReleaseBaseline : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ImdbFiles",
            columns: table => new
            {
                ImdbId = table.Column<string>(type: "text", nullable: false),
                Category = table.Column<string>(type: "text", nullable: true),
                Title = table.Column<string>(type: "text", nullable: true),
                Adult = table.Column<bool>(type: "boolean", nullable: false),
                Year = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImdbFiles", x => x.ImdbId);
            });

        migrationBuilder.CreateTable(
            name: "Torrents",
            columns: table => new
            {
                InfoHash = table.Column<string>(type: "text", nullable: false),
                Resolution = table.Column<List<string>>(type: "text[]", nullable: true),
                Year = table.Column<int>(type: "integer", nullable: true),
                Remastered = table.Column<bool>(type: "boolean", nullable: true),
                Codec = table.Column<List<string>>(type: "text[]", nullable: true),
                Audio = table.Column<List<string>>(type: "text[]", nullable: true),
                Quality = table.Column<List<string>>(type: "text[]", nullable: true),
                Episodes = table.Column<List<int>>(type: "integer[]", nullable: true),
                Seasons = table.Column<List<int>>(type: "integer[]", nullable: true),
                Languages = table.Column<List<string>>(type: "text[]", nullable: true),
                Title = table.Column<string>(type: "text", nullable: true),
                RawTitle = table.Column<string>(type: "text", nullable: true),
                Size = table.Column<long>(type: "bigint", nullable: false),
                Category = table.Column<string>(type: "text", nullable: false),
                ImdbId = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Torrents", x => x.InfoHash);
                table.ForeignKey(
                    name: "FK_Torrents_ImdbFiles_ImdbId",
                    column: x => x.ImdbId,
                    principalTable: "ImdbFiles",
                    principalColumn: "ImdbId");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ImdbFiles_ImdbId",
            table: "ImdbFiles",
            column: "ImdbId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Torrents_ImdbId",
            table: "Torrents",
            column: "ImdbId");

        migrationBuilder.CreateIndex(
            name: "IX_Torrents_InfoHash",
            table: "Torrents",
            column: "InfoHash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Torrents");

        migrationBuilder.DropTable(
            name: "ImdbFiles");
    }
}
