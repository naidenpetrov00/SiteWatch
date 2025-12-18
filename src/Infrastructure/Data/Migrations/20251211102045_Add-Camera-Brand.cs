using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Brand",
                table: "Cameras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Cameras",
                type: "nvarchar(39)",
                maxLength: 39,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Cameras",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Cameras",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "Cameras",
                type: "int",
                maxLength: 5,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Cameras",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Cameras");
        }
    }
}
