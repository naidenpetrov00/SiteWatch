using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTypesAndOfferProductRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProductRequirements_Products_ProductId",
                table: "ActivityProductRequirements");

            migrationBuilder.DropTable(
                name: "OfferProductContributions");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OfferProductLines_OfferId_ProductId",
                table: "OfferProductLines");

            migrationBuilder.DropIndex(
                name: "IX_ActivityProductRequirements_SectionId_ProductId",
                table: "ActivityProductRequirements");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ProposalProductLines");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OfferProductLines");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ActivityProductRequirements",
                newName: "ProductTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityProductRequirements_ProductId",
                table: "ActivityProductRequirements",
                newName: "IX_ActivityProductRequirements_ProductTypeId");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ProposalProductLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "ProposalProductLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTypeId",
                table: "ProposalProductLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProductTypeName",
                table: "ProposalProductLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTypeId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "OfferProductLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "OfferProductLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferProductRequirementId",
                table: "OfferProductLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTypeId",
                table: "OfferProductLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProductTypeName",
                table: "OfferProductLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTypes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferProductRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferActivitySectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceRequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConfiguredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    QuantityBehavior = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CalculatedQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferProductRequirements", x => x.Id);
                    table.UniqueConstraint("AK_OfferProductRequirements_Id_ProductTypeId", x => new { x.Id, x.ProductTypeId });
                    table.ForeignKey(
                        name: "FK_OfferProductRequirements_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferProductRequirements_OfferActivitySections_OfferActivitySectionId",
                        column: x => x.OfferActivitySectionId,
                        principalTable: "OfferActivitySections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferProductRequirements_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductTypeId",
                table: "Products",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductLines_OfferProductRequirementId",
                table: "OfferProductLines",
                column: "OfferProductRequirementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductLines_OfferProductRequirementId_ProductTypeId",
                table: "OfferProductLines",
                columns: new[] { "OfferProductRequirementId", "ProductTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NormalizedName",
                table: "Categories",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductRequirements_CategoryId",
                table: "OfferProductRequirements",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductRequirements_OfferActivitySectionId_SortOrder",
                table: "OfferProductRequirements",
                columns: new[] { "OfferActivitySectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductRequirements_OfferActivitySectionId_SourceRequirementId",
                table: "OfferProductRequirements",
                columns: new[] { "OfferActivitySectionId", "SourceRequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductRequirements_ProductTypeId",
                table: "OfferProductRequirements",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_CategoryId_NormalizedName",
                table: "ProductTypes",
                columns: new[] { "CategoryId", "NormalizedName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProductRequirements_ProductTypes_ProductTypeId",
                table: "ActivityProductRequirements",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferProductLines_OfferProductRequirements_OfferProductRequirementId_ProductTypeId",
                table: "OfferProductLines",
                columns: new[] { "OfferProductRequirementId", "ProductTypeId" },
                principalTable: "OfferProductRequirements",
                principalColumns: new[] { "Id", "ProductTypeId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProductRequirements_ProductTypes_ProductTypeId",
                table: "ActivityProductRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferProductLines_OfferProductRequirements_OfferProductRequirementId_ProductTypeId",
                table: "OfferProductLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "OfferProductRequirements");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductTypeId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OfferProductLines_OfferProductRequirementId",
                table: "OfferProductLines");

            migrationBuilder.DropIndex(
                name: "IX_OfferProductLines_OfferProductRequirementId_ProductTypeId",
                table: "OfferProductLines");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ProposalProductLines");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "ProposalProductLines");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "ProposalProductLines");

            migrationBuilder.DropColumn(
                name: "ProductTypeName",
                table: "ProposalProductLines");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "OfferProductLines");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "OfferProductLines");

            migrationBuilder.DropColumn(
                name: "OfferProductRequirementId",
                table: "OfferProductLines");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "OfferProductLines");

            migrationBuilder.DropColumn(
                name: "ProductTypeName",
                table: "OfferProductLines");

            migrationBuilder.RenameColumn(
                name: "ProductTypeId",
                table: "ActivityProductRequirements",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityProductRequirements_ProductTypeId",
                table: "ActivityProductRequirements",
                newName: "IX_ActivityProductRequirements_ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ProposalProductLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "OfferProductLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OfferProductContributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferActivitySectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferProductLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculatedQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    ConfiguredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuantityBehavior = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SourceRequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferProductContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferProductContributions_OfferActivitySections_OfferActivitySectionId",
                        column: x => x.OfferActivitySectionId,
                        principalTable: "OfferActivitySections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferProductContributions_OfferProductLines_OfferProductLineId",
                        column: x => x.OfferProductLineId,
                        principalTable: "OfferProductLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductLines_OfferId_ProductId",
                table: "OfferProductLines",
                columns: new[] { "OfferId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductRequirements_SectionId_ProductId",
                table: "ActivityProductRequirements",
                columns: new[] { "SectionId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductContributions_OfferActivitySectionId_SortOrder",
                table: "OfferProductContributions",
                columns: new[] { "OfferActivitySectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductContributions_OfferActivitySectionId_SourceRequirementId",
                table: "OfferProductContributions",
                columns: new[] { "OfferActivitySectionId", "SourceRequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductContributions_OfferProductLineId",
                table: "OfferProductContributions",
                column: "OfferProductLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProductRequirements_Products_ProductId",
                table: "ActivityProductRequirements",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
