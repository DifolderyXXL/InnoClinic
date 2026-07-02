using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedReservationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<uint>(
                name: "TimeSlotSize",
                table: "ServiceCategories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "ReservedTimeWindows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartSlotIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    SlotCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservedTimeWindows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservedTimeWindows_Date_StartSlotIndex_SlotCount",
                table: "ReservedTimeWindows",
                columns: new[] { "Date", "StartSlotIndex", "SlotCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservedTimeWindows");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "Services",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TimeSlotSize",
                table: "ServiceCategories",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }
    }
}
