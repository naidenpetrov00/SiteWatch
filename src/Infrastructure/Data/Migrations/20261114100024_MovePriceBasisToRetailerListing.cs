using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovePriceBasisToRetailerListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceBasis",
                table: "RetailerExtractionRules");

            migrationBuilder.AddColumn<string>(
                name: "OnlinePriceBasis",
                table: "RetailerListings",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlinePriceBasis",
                table: "RetailerListings");

            migrationBuilder.AddColumn<string>(
                name: "PriceBasis",
                table: "RetailerExtractionRules",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }
    }
}
