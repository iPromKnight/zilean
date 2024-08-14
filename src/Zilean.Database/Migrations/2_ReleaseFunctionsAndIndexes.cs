using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;
using Zilean.Database.Indexes;
using Zilean.Database.Triggers;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class ReleaseFunctionsAndIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("SET pg_trgm.similarity_threshold = 0.85;");

        migrationBuilder.Sql(OnTorrentInsertFileImdbId.RemoveTrigger);
        migrationBuilder.Sql(TorrentsFetchImdbId.RemoveProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveImdbProcedure);
        migrationBuilder.Sql(ImdbFilesIndexes.RemoveIndexes);

        migrationBuilder.Sql(SearchImdbProcedure.CreateImdbProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.CreateTorrentProcedure);
        migrationBuilder.Sql(TorrentsFetchImdbId.CreateProcedure);
        migrationBuilder.Sql(OnTorrentInsertFileImdbId.CreateTrigger);
        migrationBuilder.Sql(ImdbFilesIndexes.CreateIndexes);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(OnTorrentInsertFileImdbId.RemoveTrigger);
        migrationBuilder.Sql(TorrentsFetchImdbId.RemoveProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveTorrentProcedure);
        migrationBuilder.Sql(SearchImdbProcedure.RemoveImdbProcedure);
        migrationBuilder.Sql(ImdbFilesIndexes.RemoveIndexes);
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}
