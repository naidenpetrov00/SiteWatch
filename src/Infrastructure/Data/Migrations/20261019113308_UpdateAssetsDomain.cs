using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssetsDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetEvents_AssetCatalogItems_PreviousCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetEvents_AssetCatalogItems_ResultingCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetCatalogItems_CatalogItemId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "AssetCatalogItems");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CatalogItemId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AssetEvents_PreviousCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.DropIndex(
                name: "IX_AssetEvents_ResultingCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "CatalogItemId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "PreviousCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingCatalogItemId",
                table: "AssetEvents");

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "Assets",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Assets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Assets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Assets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousClassification",
                table: "AssetEvents",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousDescription",
                table: "AssetEvents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousManufacturer",
                table: "AssetEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousModel",
                table: "AssetEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousName",
                table: "AssetEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultingClassification",
                table: "AssetEvents",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultingDescription",
                table: "AssetEvents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultingManufacturer",
                table: "AssetEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultingModel",
                table: "AssetEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultingName",
                table: "AssetEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Classification",
                table: "Assets",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Classification_Name_NumberId",
                table: "Assets",
                columns: new[] { "Classification", "Name", "NumberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Manufacturer",
                table: "Assets",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Model",
                table: "Assets",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Name",
                table: "Assets",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_Classification",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Classification_Name_NumberId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Manufacturer",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Model",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Name",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "PreviousClassification",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "PreviousDescription",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "PreviousManufacturer",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "PreviousModel",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "PreviousName",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingClassification",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingDescription",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingManufacturer",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingModel",
                table: "AssetEvents");

            migrationBuilder.DropColumn(
                name: "ResultingName",
                table: "AssetEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogItemId",
                table: "Assets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousCatalogItemId",
                table: "AssetEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResultingCatalogItemId",
                table: "AssetEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AssetCatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Classification = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCatalogItems", x => x.Id);
                    table.CheckConstraint("CK_AssetCatalogItems_ReviewState", "([ReviewStatus] = N'PendingReview' AND [ReviewedById] IS NULL AND [ReviewedAt] IS NULL AND [RejectionReason] IS NULL) OR ([ReviewStatus] = N'Active' AND [ReviewedById] IS NOT NULL AND [ReviewedAt] IS NOT NULL AND [RejectionReason] IS NULL) OR ([ReviewStatus] = N'Rejected' AND [ReviewedById] IS NOT NULL AND [ReviewedAt] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_AssetCatalogItems_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CatalogItemId",
                table: "Assets",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_PreviousCatalogItemId",
                table: "AssetEvents",
                column: "PreviousCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ResultingCatalogItemId",
                table: "AssetEvents",
                column: "ResultingCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCatalogItems_Classification",
                table: "AssetCatalogItems",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCatalogItems_Name",
                table: "AssetCatalogItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCatalogItems_ReviewedById",
                table: "AssetCatalogItems",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCatalogItems_ReviewStatus",
                table: "AssetCatalogItems",
                column: "ReviewStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetEvents_AssetCatalogItems_PreviousCatalogItemId",
                table: "AssetEvents",
                column: "PreviousCatalogItemId",
                principalTable: "AssetCatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetEvents_AssetCatalogItems_ResultingCatalogItemId",
                table: "AssetEvents",
                column: "ResultingCatalogItemId",
                principalTable: "AssetCatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetCatalogItems_CatalogItemId",
                table: "Assets",
                column: "CatalogItemId",
                principalTable: "AssetCatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
