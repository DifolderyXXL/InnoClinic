using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class OverlappintConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");
            
            migrationBuilder.DropIndex(
                name: "IX_ReservedTimeWindows_Date_StartSlotIndex_SlotCount",
                table: "ReservedTimeWindows");

            migrationBuilder.CreateIndex(
                name: "IX_ReservedTimeWindows_DoctorId_Date",
                table: "ReservedTimeWindows",
                columns: new[] { "DoctorId", "Date" });
            
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ReservedTimeWindows""
                ADD CONSTRAINT no_overlap
                EXCLUDE USING GIST (
                    ""DoctorId"" WITH =,
                    ""Date"" WITH =,
                    int4range(""StartSlotIndex"", ""StartSlotIndex"" + ""SlotCount"") WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservedTimeWindows_DoctorId_Date",
                table: "ReservedTimeWindows");

            migrationBuilder.CreateIndex(
                name: "IX_ReservedTimeWindows_Date_StartSlotIndex_SlotCount",
                table: "ReservedTimeWindows",
                columns: new[] { "Date", "StartSlotIndex", "SlotCount" });
            
            migrationBuilder.Sql(@"ALTER TABLE ""ReservedTimeWindows"" DROP CONSTRAINT IF EXISTS no_overlap;");
            
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");
        }
    }
}
