using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OfferGenerationAfterMerging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "ProposalNumberIds",
                schema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryClientUserId",
                table: "Sites",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActivityDiscountPercentage",
                table: "Offers",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductDiscountPercentage",
                table: "Offers",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAmount",
                table: "OfferActivitySections",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingMode",
                table: "OfferActivitySections",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ProposalNumberIds]"),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferNumberId = table.Column<int>(type: "int", nullable: false),
                    SourceOfferFinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SiteNumberId = table.Column<int>(type: "int", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientDisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    PublicNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActivitySubtotalBeforeDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActivityDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ActivityDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActivityTotalAfterDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductSubtotalBeforeDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProductDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductTotalAfterDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnpricedOptionalItemCount = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FirstViewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RespondedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResponseComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                    table.CheckConstraint("CK_Proposals_Discounts", "[ActivityDiscountPercentage] >= 0 AND [ActivityDiscountPercentage] <= 100 AND [ProductDiscountPercentage] >= 0 AND [ProductDiscountPercentage] <= 100");
                    table.CheckConstraint("CK_Proposals_InteractionTimestamps", "([FirstViewedAt] IS NULL OR ([IssuedAt] IS NOT NULL AND [FirstViewedAt] >= [IssuedAt])) AND ([RespondedAt] IS NULL OR ([IssuedAt] IS NOT NULL AND [RespondedAt] >= [IssuedAt]))");
                    table.CheckConstraint("CK_Proposals_ResponseMetadata", "(([Status] IN (N'Draft', N'Issued') AND [RespondedAt] IS NULL AND [RespondedByUserId] IS NULL AND [ResponseComment] IS NULL) OR ([Status] IN (N'Accepted', N'Rejected') AND [RespondedAt] IS NOT NULL AND [RespondedByUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_Proposals_RevisionNumber", "[RevisionNumber] > 0");
                    table.CheckConstraint("CK_Proposals_UnpricedOptionalItemCount", "[UnpricedOptionalItemCount] >= 0");
                    table.ForeignKey(
                        name: "FK_Proposals_Offers_SourceOfferId",
                        column: x => x.SourceOfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProposalActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityNumberId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalActivities_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StoredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalDocuments_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalProductLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferProductLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNumberId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PackageUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    OptionalQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    SelectedRetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedRetailerDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SelectedPriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SelectedPriceCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SelectedPriceBasis = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    RequiredTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OptionalTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsOptionalPriceExcluded = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalProductLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalProductLines_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalActivitySections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferActivitySectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BasisQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedMeasurement = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PricingMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CalculatedTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalActivitySections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalActivitySections_ProposalActivities_ProposalActivityId",
                        column: x => x.ProposalActivityId,
                        principalTable: "ProposalActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_PrimaryClientUserId",
                table: "Sites",
                column: "PrimaryClientUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Offers_ActivityDiscountPercentage",
                table: "Offers",
                sql: "[ActivityDiscountPercentage] >= 0 AND [ActivityDiscountPercentage] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Offers_ProductDiscountPercentage",
                table: "Offers",
                sql: "[ProductDiscountPercentage] >= 0 AND [ProductDiscountPercentage] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OfferActivitySections_Pricing",
                table: "OfferActivitySections",
                sql: "([PricingMode] IS NULL AND [PriceAmount] IS NULL) OR ([PricingMode] = N'free' AND [PriceAmount] IS NULL) OR ([PricingMode] IN (N'fixed', N'per-measurement') AND [PriceAmount] > 0)");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalActivities_ProposalId_SortOrder",
                table: "ProposalActivities",
                columns: new[] { "ProposalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalActivitySections_ProposalActivityId_SortOrder",
                table: "ProposalActivitySections",
                columns: new[] { "ProposalActivityId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDocuments_BlobName",
                table: "ProposalDocuments",
                column: "BlobName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDocuments_ProposalId",
                table: "ProposalDocuments",
                column: "ProposalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposalProductLines_ProposalId_SortOrder",
                table: "ProposalProductLines",
                columns: new[] { "ProposalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_NumberId_RevisionNumber",
                table: "Proposals",
                columns: new[] { "NumberId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SiteId_NumberId",
                table: "Proposals",
                columns: new[] { "SiteId", "NumberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SiteId_RecipientUserId_IssuedAt",
                table: "Proposals",
                columns: new[] { "SiteId", "RecipientUserId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SourceOfferId",
                table: "Proposals",
                column: "SourceOfferId",
                unique: true,
                filter: "[Status] = N'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SourceOfferId_RevisionNumber",
                table: "Proposals",
                columns: new[] { "SourceOfferId", "RevisionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_AspNetUsers_PrimaryClientUserId",
                table: "Sites",
                column: "PrimaryClientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_AspNetUsers_PrimaryClientUserId",
                table: "Sites");

            migrationBuilder.DropTable(
                name: "ProposalActivitySections");

            migrationBuilder.DropTable(
                name: "ProposalDocuments");

            migrationBuilder.DropTable(
                name: "ProposalProductLines");

            migrationBuilder.DropTable(
                name: "ProposalActivities");

            migrationBuilder.DropTable(
                name: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Sites_PrimaryClientUserId",
                table: "Sites");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Offers_ActivityDiscountPercentage",
                table: "Offers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Offers_ProductDiscountPercentage",
                table: "Offers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OfferActivitySections_Pricing",
                table: "OfferActivitySections");

            migrationBuilder.DropColumn(
                name: "PrimaryClientUserId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ActivityDiscountPercentage",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ProductDiscountPercentage",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "PriceAmount",
                table: "OfferActivitySections");

            migrationBuilder.DropColumn(
                name: "PricingMode",
                table: "OfferActivitySections");

            migrationBuilder.DropSequence(
                name: "ProposalNumberIds",
                schema: "dbo");
        }
    }
}
