using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetTrackingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateSequence<int>(
                name: "AssetNumberIds",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "AssetCatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReviewedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AssetNumberIds]"),
                    QrIdentifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturerSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibleWorkerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PendingHandoverRecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PreHandoverOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    HistorySequence = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.CheckConstraint("CK_Assets_HandoverRecipient", "[PendingHandoverRecipientId] IS NULL OR [PendingHandoverRecipientId] <> [ResponsibleWorkerId]");
                    table.CheckConstraint("CK_Assets_HandoverState", "([OperationalStatus] = N'InTransfer' AND [ResponsibleWorkerId] IS NOT NULL AND [PendingHandoverRecipientId] IS NOT NULL AND [PreHandoverOperationalStatus] IS NOT NULL AND [PreHandoverOperationalStatus] NOT IN (N'InTransfer', N'Missing', N'Retired')) OR ([OperationalStatus] <> N'InTransfer' AND [PendingHandoverRecipientId] IS NULL AND [PreHandoverOperationalStatus] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Assets_AspNetUsers_PendingHandoverRecipientId",
                        column: x => x.PendingHandoverRecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_AspNetUsers_ResponsibleWorkerId",
                        column: x => x.ResponsibleWorkerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_AssetCatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "AssetCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreviousCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousQrIdentifier = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousManufacturerSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousResponsibleWorkerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PreviousPendingHandoverRecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PreviousCondition = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PreviousOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PreviousPreHandoverOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PreviousReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ResultingCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultingQrIdentifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultingManufacturerSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultingSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultingResponsibleWorkerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResultingPendingHandoverRecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResultingCondition = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ResultingOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ResultingPreHandoverOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ResultingReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AspNetUsers_PreviousPendingHandoverRecipientId",
                        column: x => x.PreviousPendingHandoverRecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AspNetUsers_PreviousResponsibleWorkerId",
                        column: x => x.PreviousResponsibleWorkerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AspNetUsers_ResultingPendingHandoverRecipientId",
                        column: x => x.ResultingPendingHandoverRecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AspNetUsers_ResultingResponsibleWorkerId",
                        column: x => x.ResultingResponsibleWorkerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AssetCatalogItems_PreviousCatalogItemId",
                        column: x => x.PreviousCatalogItemId,
                        principalTable: "AssetCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_AssetCatalogItems_ResultingCatalogItemId",
                        column: x => x.ResultingCatalogItemId,
                        principalTable: "AssetCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_Sites_PreviousSiteId",
                        column: x => x.PreviousSiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetEvents_Sites_ResultingSiteId",
                        column: x => x.ResultingSiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ActorId",
                table: "AssetEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_AssetId_OccurredAt_SequenceNumber",
                table: "AssetEvents",
                columns: new[] { "AssetId", "OccurredAt", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_AssetId_SequenceNumber",
                table: "AssetEvents",
                columns: new[] { "AssetId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_PreviousCatalogItemId",
                table: "AssetEvents",
                column: "PreviousCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_PreviousPendingHandoverRecipientId",
                table: "AssetEvents",
                column: "PreviousPendingHandoverRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_PreviousResponsibleWorkerId",
                table: "AssetEvents",
                column: "PreviousResponsibleWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_PreviousSiteId",
                table: "AssetEvents",
                column: "PreviousSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ResultingCatalogItemId",
                table: "AssetEvents",
                column: "ResultingCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ResultingPendingHandoverRecipientId",
                table: "AssetEvents",
                column: "ResultingPendingHandoverRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ResultingResponsibleWorkerId",
                table: "AssetEvents",
                column: "ResultingResponsibleWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEvents_ResultingSiteId",
                table: "AssetEvents",
                column: "ResultingSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CatalogItemId",
                table: "Assets",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Condition",
                table: "Assets",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ManufacturerSerialNumber",
                table: "Assets",
                column: "ManufacturerSerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_NumberId",
                table: "Assets",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_OperationalStatus",
                table: "Assets",
                column: "OperationalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_PendingHandoverRecipientId",
                table: "Assets",
                column: "PendingHandoverRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_QrIdentifier",
                table: "Assets",
                column: "QrIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ResponsibleWorkerId",
                table: "Assets",
                column: "ResponsibleWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ReviewStatus",
                table: "Assets",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SiteId",
                table: "Assets",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SiteId_OperationalStatus_NumberId",
                table: "Assets",
                columns: new[] { "SiteId", "OperationalStatus", "NumberId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetEvents");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AssetCatalogItems");

            migrationBuilder.DropSequence(
                name: "AssetNumberIds",
                schema: "dbo");

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
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActivityDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ActivitySubtotalBeforeDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActivityTotalAfterDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FirstViewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ProposalNumberIds]"),
                    PaymentTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProductSubtotalBeforeDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductTotalAfterDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecipientDisplayName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RespondedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResponseComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SiteAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteNumberId = table.Column<int>(type: "int", nullable: false),
                    SourceOfferFinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourceOfferNumberId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnpricedOptionalItemCount = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true)
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
                    ActivityNumberId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SourceOfferActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
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
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOptionalPriceExcluded = table.Column<bool>(type: "bit", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OptionalQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    OptionalTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PackageUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNumberId = table.Column<int>(type: "int", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    RequiredTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SelectedPriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SelectedPriceBasis = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    SelectedPriceCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SelectedRetailerDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SelectedRetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SourceOfferProductLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                    BasisQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculatedTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PricingMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedMeasurement = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SourceOfferActivitySectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
    }
}
