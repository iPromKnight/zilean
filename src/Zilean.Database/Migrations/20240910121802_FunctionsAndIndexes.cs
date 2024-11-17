using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;
using Zilean.Database.Indexes;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class FunctionsAndIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("SET pg_trgm.similarity_threshold = 0.85;");

        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveImdbProcedure);
        migrationBuilder.Sql(ImdbFilesIndexes.RemoveIndexes);

        migrationBuilder.Sql(SearchImdbProcedure.CreateImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedure);
        migrationBuilder.Sql(ImdbFilesIndexes.CreateIndexes);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveImdbProcedure);
        migrationBuilder.Sql(ImdbFilesIndexes.RemoveIndexes);
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}
