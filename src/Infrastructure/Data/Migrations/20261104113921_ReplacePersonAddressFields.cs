using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePersonAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "PersonAddresses");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "PersonAddresses",
                newName: "AddressDetails");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "PersonAddresses",
                newName: "Settlement");

            migrationBuilder.RenameColumn(
                name: "AdditionalLine",
                table: "PersonAddresses",
                newName: "Street");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Apartment",
                table: "PersonAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "PersonAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Entrance",
                table: "PersonAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                table: "PersonAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipality",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "PersonAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Quarter",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apartment",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Building",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "District",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Entrance",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Municipality",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "PersonAddresses");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "PersonAddresses",
                newName: "AdditionalLine");

            migrationBuilder.RenameColumn(
                name: "Settlement",
                table: "PersonAddresses",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "AddressDetails",
                table: "PersonAddresses",
                newName: "Details");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "PersonAddresses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
