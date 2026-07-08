using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    ReservationId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartSlotIndex = table.Column<int>(type: "integer", nullable: false),
                    SlotCount = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PatientAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    ReservationId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartSlotIndex = table.Column<int>(type: "integer", nullable: false),
                    SlotCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentStates", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AppointmentStates");
        }
    }
}
