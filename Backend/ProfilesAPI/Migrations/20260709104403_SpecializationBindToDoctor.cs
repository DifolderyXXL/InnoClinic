using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfilesAPI.Migrations
{
    /// <inheritdoc />
    public partial class SpecializationBindToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_Id",
                table: "Patients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Patients_Id",
                table: "Patients",
                column: "Id");
        }
    }
}
