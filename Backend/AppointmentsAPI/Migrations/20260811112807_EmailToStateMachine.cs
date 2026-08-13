using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class EmailToStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCreatedByAdmin",
                table: "AppointmentStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PatientEmail",
                table: "AppointmentStates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCreatedByAdmin",
                table: "AppointmentStates");

            migrationBuilder.DropColumn(
                name: "PatientEmail",
                table: "AppointmentStates");
        }
    }
}
