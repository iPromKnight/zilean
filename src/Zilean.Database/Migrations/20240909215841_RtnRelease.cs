using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class RtnRelease : Migration
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
                RawTitle = table.Column<string>(type: "text", nullable: false),
                ParsedTitle = table.Column<string>(type: "text", nullable: false),
                NormalizedTitle = table.Column<string>(type: "text", nullable: false),
                Trash = table.Column<bool>(type: "boolean", nullable: false),
                Year = table.Column<int>(type: "integer", nullable: true),
                Resolution = table.Column<string>(type: "text", nullable: false),
                Seasons = table.Column<int[]>(type: "integer[]", nullable: false),
                Episodes = table.Column<int[]>(type: "integer[]", nullable: false),
                Complete = table.Column<bool>(type: "boolean", nullable: false),
                Volumes = table.Column<int[]>(type: "integer[]", nullable: false),
                Languages = table.Column<string[]>(type: "text[]", nullable: false),
                Quality = table.Column<string>(type: "text", nullable: true),
                Hdr = table.Column<string[]>(type: "text[]", nullable: false),
                Codec = table.Column<string>(type: "text", nullable: true),
                Audio = table.Column<string[]>(type: "text[]", nullable: false),
                Channels = table.Column<string[]>(type: "text[]", nullable: false),
                Dubbed = table.Column<bool>(type: "boolean", nullable: false),
                Subbed = table.Column<bool>(type: "boolean", nullable: false),
                Date = table.Column<string>(type: "text", nullable: true),
                Group = table.Column<string>(type: "text", nullable: true),
                Edition = table.Column<string>(type: "text", nullable: true),
                BitDepth = table.Column<string>(type: "text", nullable: true),
                Bitrate = table.Column<string>(type: "text", nullable: true),
                Network = table.Column<string>(type: "text", nullable: true),
                Extended = table.Column<bool>(type: "boolean", nullable: false),
                Converted = table.Column<bool>(type: "boolean", nullable: false),
                Hardcoded = table.Column<bool>(type: "boolean", nullable: false),
                Region = table.Column<string>(type: "text", nullable: true),
                Ppv = table.Column<bool>(type: "boolean", nullable: false),
                Is3d = table.Column<bool>(type: "boolean", nullable: false),
                Site = table.Column<string>(type: "text", nullable: true),
                Size = table.Column<string>(type: "text", nullable: true),
                Proper = table.Column<bool>(type: "boolean", nullable: false),
                Repack = table.Column<bool>(type: "boolean", nullable: false),
                Retail = table.Column<bool>(type: "boolean", nullable: false),
                Upscaled = table.Column<bool>(type: "boolean", nullable: false),
                Remastered = table.Column<bool>(type: "boolean", nullable: false),
                Unrated = table.Column<bool>(type: "boolean", nullable: false),
                Documentary = table.Column<bool>(type: "boolean", nullable: false),
                EpisodeCode = table.Column<string>(type: "text", nullable: true),
                Country = table.Column<string>(type: "text", nullable: true),
                Container = table.Column<string>(type: "text", nullable: true),
                Extension = table.Column<string>(type: "text", nullable: true),
                Torrent = table.Column<bool>(type: "boolean", nullable: false),
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
