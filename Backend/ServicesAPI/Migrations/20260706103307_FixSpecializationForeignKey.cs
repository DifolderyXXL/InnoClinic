using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixSpecializationForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Specializations_CategoryId",
                table: "Services");

            migrationBuilder.CreateIndex(
                name: "IX_Services_SpecializationId",
                table: "Services",
                column: "SpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_SpecializationId",
                table: "Services");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specializations_CategoryId",
                table: "Services",
                column: "CategoryId",
                principalTable: "Specializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
