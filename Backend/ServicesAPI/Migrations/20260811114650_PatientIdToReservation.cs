using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class PatientIdToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "ReservedTimeWindows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ReservedTimeWindows""
                ADD CONSTRAINT no_patient_overlap
                EXCLUDE USING GIST (
                    ""PatientId"" WITH =,
                    ""Date"" WITH =,
                    int4range(""StartSlotIndex"", ""StartSlotIndex"" + ""SlotCount"") WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "ReservedTimeWindows");
        }
    }
}
