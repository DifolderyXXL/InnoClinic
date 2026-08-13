using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class IndexForPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReservedTimeWindows_PatientId_Date",
                table: "ReservedTimeWindows",
                columns: new[] { "PatientId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservedTimeWindows_PatientId_Date",
                table: "ReservedTimeWindows");
        }
    }
}
