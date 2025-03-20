using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReCollect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Localization_City",
                table: "PackingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localization_Country",
                table: "PackingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_Value",
                table: "PackingLists",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localization_City",
                table: "PackingLists");

            migrationBuilder.DropColumn(
                name: "Localization_Country",
                table: "PackingLists");

            migrationBuilder.DropColumn(
                name: "Name_Value",
                table: "PackingLists");
        }
    }
}
