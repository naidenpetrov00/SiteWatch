using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Port",
                table: "Cameras",
                newName: "RtspPort");

            migrationBuilder.AddColumn<int>(
                name: "PtzPort",
                table: "Cameras",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PtzPort",
                table: "Cameras");

            migrationBuilder.RenameColumn(
                name: "RtspPort",
                table: "Cameras",
                newName: "Port");
        }
    }
}
