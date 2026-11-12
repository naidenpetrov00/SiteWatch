using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePersonEgnAndEikIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_Egn",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_Eik",
                table: "Persons");

            migrationBuilder.CreateIndex(
                name: "UX_Persons_Egn",
                table: "Persons",
                column: "Egn",
                unique: true,
                filter: "[Egn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Persons_Eik",
                table: "Persons",
                column: "Eik",
                unique: true,
                filter: "[Eik] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Persons_Egn",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "UX_Persons_Eik",
                table: "Persons");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Egn",
                table: "Persons",
                column: "Egn");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Eik",
                table: "Persons",
                column: "Eik");
        }
    }
}
