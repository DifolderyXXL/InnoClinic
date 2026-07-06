using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReservationTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "ReservedTimeWindows",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "ReservedTimeWindows");
        }
    }
}
