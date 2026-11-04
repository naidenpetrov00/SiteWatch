using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultOfferRetailers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "OfferRetailerComparisons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DefaultOfferRetailers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultOfferRetailers", x => x.Id);
                    table.CheckConstraint("CK_DefaultOfferRetailers_SortOrder", "[SortOrder] >= 0 AND [SortOrder] < 3");
                    table.ForeignKey(
                        name: "FK_DefaultOfferRetailers_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultOfferRetailers_RetailerId",
                table: "DefaultOfferRetailers",
                column: "RetailerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefaultOfferRetailers_SortOrder",
                table: "DefaultOfferRetailers",
                column: "SortOrder",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultOfferRetailers");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "OfferRetailerComparisons");
        }
    }
}
