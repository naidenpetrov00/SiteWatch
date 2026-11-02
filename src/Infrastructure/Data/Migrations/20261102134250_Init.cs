using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "ActivityNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "ApplicationUserNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "AssetNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "CameraNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "InvoiceNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "IssueNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "OfferNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "PersonNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "ProductNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "ProposalNumberIds",
                schema: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "SiteNumberIds",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "ActivityCatalogNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    NodeType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: true, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivityNumberIds]"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCatalogNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityCatalogNodes_ActivityCatalogNodes_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "ActivityCatalogNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ApplicationUserNumberIds]"),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

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
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[PersonNumberIds]"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LegalForm = table.Column<int>(type: "int", nullable: true),
                    Egn = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Eik = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SearchName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SearchTaxIdentifier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetailerPriceCollectionWorkerLeases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerPriceCollectionWorkerLeases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityRequirementSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BasisQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRequirementSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityRequirementSections_ActivityCatalogNodes_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityCatalogNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SiteNumberIds]"),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MediaPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrimaryClientUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsCompanyStock = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sites_AspNetUsers_PrimaryClientUserId",
                        column: x => x.PrimaryClientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "PersonAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdditionalLine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonAddresses_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    BIC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonBankAccounts_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonContacts_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Retailers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CompanyPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseWebsiteUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    NormalizedWebsiteHost = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retailers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Retailers_Persons_CompanyPersonId",
                        column: x => x.CompanyPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserSite",
                columns: table => new
                {
                    SitesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserSite", x => new { x.SitesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserSite_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserSite_Sites_SitesId",
                        column: x => x.SitesId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AssetNumberIds]"),
                    QrIdentifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                        name: "FK_Assets_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CameraNumberIds]"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(39)", maxLength: 39, nullable: true),
                    RtspPort = table.Column<int>(type: "int", maxLength: 5, nullable: true),
                    PtzPort = table.Column<int>(type: "int", nullable: true),
                    Protocol = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cameras_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedFromSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[InvoiceNumberIds]"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Eik = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentTerm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalValueExcludingVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VatRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalValueIncludingVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaymentTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Persons_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Sites_SubmittedFromSiteId",
                        column: x => x.SubmittedFromSiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[IssueNumberIds]"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[OfferNumberIds]"),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ActivityDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    ProductDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FinalizedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.CheckConstraint("CK_Offers_ActivityDiscountPercentage", "[ActivityDiscountPercentage] >= 0 AND [ActivityDiscountPercentage] <= 100");
                    table.CheckConstraint("CK_Offers_ProductDiscountPercentage", "[ProductDiscountPercentage] >= 0 AND [ProductDiscountPercentage] <= 100");
                    table.ForeignKey(
                        name: "FK_Offers_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteFiles",
                columns: table => new
                {
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteFiles", x => new { x.SiteId, x.FileId });
                    table.ForeignKey(
                        name: "FK_SiteFiles_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteImages",
                columns: table => new
                {
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThumbnailImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteImages", x => new { x.SiteId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_SiteImages_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteVideos",
                columns: table => new
                {
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteVideos", x => new { x.SiteId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_SiteVideos_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityProductRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    QuantityBehavior = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityProductRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityProductRequirements_ActivityRequirementSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ActivityRequirementSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityProductRequirements_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ProductNumberIds]"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PackageUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SearchConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SearchIdentity = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
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
                    PreviousClassification = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PreviousName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreviousManufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreviousQrIdentifier = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousManufacturerSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousResponsibleWorkerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PreviousPendingHandoverRecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PreviousCondition = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PreviousOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PreviousPreHandoverOperationalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PreviousReviewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ResultingClassification = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ResultingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResultingManufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultingModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultingDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "SitePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SitePayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SitePayments_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PreviewBlobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueAttachments_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueWorkers",
                columns: table => new
                {
                    AssignedIssuesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedWorkersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueWorkers", x => new { x.AssignedIssuesId, x.AssignedWorkersId });
                    table.ForeignKey(
                        name: "FK_IssueWorkers_AspNetUsers_AssignedWorkersId",
                        column: x => x.AssignedWorkersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IssueWorkers_Issues_AssignedIssuesId",
                        column: x => x.AssignedIssuesId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityNumberId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferActivities_ActivityCatalogNodes_SourceActivityId",
                        column: x => x.SourceActivityId,
                        principalTable: "ActivityCatalogNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferActivities_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferRetailerComparisons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RetailerBaseWebsiteUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferRetailerComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferRetailerComparisons_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferRetailerComparisons_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "RetailerListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RetailerProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetailerListings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerListings_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferActivitySections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BasisQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedMeasurement = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PricingMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferActivitySections", x => x.Id);
                    table.CheckConstraint("CK_OfferActivitySections_Pricing", "([PricingMode] IS NULL AND [PriceAmount] IS NULL) OR ([PricingMode] = N'free' AND [PriceAmount] IS NULL) OR ([PricingMode] IN (N'fixed', N'per-measurement') AND [PriceAmount] > 0)");
                    table.ForeignKey(
                        name: "FK_OfferActivitySections_OfferActivities_OfferActivityId",
                        column: x => x.OfferActivityId,
                        principalTable: "OfferActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "OfferProductLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferProductRequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNumberId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PackageUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    OptionalQuantity = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferProductLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferProductLines_OfferProductRequirements_OfferProductRequirementId_ProductTypeId",
                        columns: x => new { x.OfferProductRequirementId, x.ProductTypeId },
                        principalTable: "OfferProductRequirements",
                        principalColumns: new[] { "Id", "ProductTypeId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferProductLines_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferProductLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferProductPriceSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferProductLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerPriceObservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RetailerProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Basis = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RecordedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SelectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SelectedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferProductPriceSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferProductPriceSelections_OfferProductLines_OfferProductLineId",
                        column: x => x.OfferProductLineId,
                        principalTable: "OfferProductLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferProductPriceSelections_RetailerListings_RetailerListingId",
                        column: x => x.RetailerListingId,
                        principalTable: "RetailerListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferProductPriceSelections_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetailerExtractionAllowedHosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtractionProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NormalizedHost = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerExtractionAllowedHosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetailerExtractionProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConfigurationRevision = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    ValidatedConfigurationRevision = table.Column<long>(type: "bigint", nullable: true),
                    LastSuccessfulTestAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSuccessfulTestRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerExtractionProfiles", x => x.Id);
                    table.CheckConstraint("CK_RetailerExtractionProfiles_ConfigurationRevision", "[ConfigurationRevision] > 0 AND ([ValidatedConfigurationRevision] IS NULL OR ([ValidatedConfigurationRevision] > 0 AND [ValidatedConfigurationRevision] <= [ConfigurationRevision]))");
                    table.CheckConstraint("CK_RetailerExtractionProfiles_Lifecycle", "(([Status] = N'Draft' AND [IsActive] = 0 AND [PublishedAt] IS NULL AND [PublishedBy] IS NULL) OR ([Status] = N'Published' AND [PublishedAt] IS NOT NULL AND [PublishedBy] IS NOT NULL AND LEN(LTRIM(RTRIM([PublishedBy]))) > 0)) AND ([IsActive] = 0 OR [Status] = N'Published')");
                    table.CheckConstraint("CK_RetailerExtractionProfiles_TestValidationMetadata", "([ValidatedConfigurationRevision] IS NULL AND [LastSuccessfulTestAt] IS NULL AND [LastSuccessfulTestRuleId] IS NULL) OR ([ValidatedConfigurationRevision] IS NOT NULL AND [LastSuccessfulTestAt] IS NOT NULL AND [LastSuccessfulTestRuleId] IS NOT NULL)");
                    table.CheckConstraint("CK_RetailerExtractionProfiles_Version", "[Version] > 0");
                    table.ForeignKey(
                        name: "FK_RetailerExtractionProfiles_Persons_CompanyPersonId",
                        column: x => x.CompanyPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetailerExtractionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtractionProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    JsonLdObjectType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JsonLdPricePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    JsonLdCurrencyPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CssSelector = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CssValueSource = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    CssAttributeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DecimalSeparator = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ThousandsSeparator = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ExpectedCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PriceBasis = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    MinimumValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaximumValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerExtractionRules", x => x.Id);
                    table.UniqueConstraint("AK_RetailerExtractionRules_ExtractionProfileId_Id", x => new { x.ExtractionProfileId, x.Id });
                    table.CheckConstraint("CK_RetailerExtractionRules_AcceptableRange", "([MinimumValue] IS NULL OR [MinimumValue] > 0) AND ([MaximumValue] IS NULL OR [MaximumValue] > 0) AND ([MinimumValue] IS NULL OR [MaximumValue] IS NULL OR [MinimumValue] <= [MaximumValue])");
                    table.CheckConstraint("CK_RetailerExtractionRules_EurCurrency", "[ExpectedCurrencyCode] = N'EUR'");
                    table.CheckConstraint("CK_RetailerExtractionRules_Priority", "[Priority] > 0");
                    table.ForeignKey(
                        name: "FK_RetailerExtractionRules_RetailerExtractionProfiles_ExtractionProfileId",
                        column: x => x.ExtractionProfileId,
                        principalTable: "RetailerExtractionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetailerPriceCollectionRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtractionProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    QueuedCount = table.Column<int>(type: "int", nullable: false),
                    RunningCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    SucceededCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerPriceCollectionRuns", x => x.Id);
                    table.CheckConstraint("CK_RetailerPriceCollectionRuns_Counters", "[TotalCount] >= 0 AND [QueuedCount] >= 0 AND [RunningCount] >= 0 AND [ProcessedCount] >= 0 AND [SucceededCount] >= 0 AND [FailedCount] >= 0 AND [SkippedCount] >= 0 AND [ProcessedCount] = [SucceededCount] + [FailedCount] + [SkippedCount] AND [TotalCount] = [QueuedCount] + [RunningCount] + [ProcessedCount]");
                    table.CheckConstraint("CK_RetailerPriceCollectionRuns_Lifecycle", "([Status] = N'queued' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL) OR ([Status] = N'running' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL) OR ([Status] = N'completed' AND [QueuedCount] = 0 AND [RunningCount] = 0 AND [FailedCount] = 0 AND [CompletedAt] IS NOT NULL) OR ([Status] = N'completedWithFailures' AND [QueuedCount] = 0 AND [RunningCount] = 0 AND [FailedCount] > 0 AND [CompletedAt] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRuns_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRuns_Persons_CompanyPersonId",
                        column: x => x.CompanyPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRuns_RetailerExtractionProfiles_ExtractionProfileId",
                        column: x => x.ExtractionProfileId,
                        principalTable: "RetailerExtractionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetailerPriceObservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Basis = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RecordedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExtractionProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MatchedExtractionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerPriceObservations", x => x.Id);
                    table.CheckConstraint("CK_RetailerPriceObservations_ExtractionProvenance", "([Source] = N'manual' AND [ExtractionProfileId] IS NULL AND [MatchedExtractionRuleId] IS NULL) OR ([Source] = N'automated' AND [ExtractionProfileId] IS NOT NULL AND [MatchedExtractionRuleId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RetailerPriceObservations_RetailerExtractionProfiles_ExtractionProfileId",
                        column: x => x.ExtractionProfileId,
                        principalTable: "RetailerExtractionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerPriceObservations_RetailerExtractionRules_ExtractionProfileId_MatchedExtractionRuleId",
                        columns: x => new { x.ExtractionProfileId, x.MatchedExtractionRuleId },
                        principalTable: "RetailerExtractionRules",
                        principalColumns: new[] { "ExtractionProfileId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerPriceObservations_RetailerListings_RetailerListingId",
                        column: x => x.RetailerListingId,
                        principalTable: "RetailerListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetailerPriceCollectionRunItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetailerListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CapturedProductUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    FinalSourceUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LeaseToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetailerPriceObservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiagnosticCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DiagnosticMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerPriceCollectionRunItems", x => x.Id);
                    table.CheckConstraint("CK_RetailerPriceCollectionRunItems_Lifecycle", "([Status] = N'queued' AND [LeaseToken] IS NULL AND [LeaseExpiresAt] IS NULL AND [CompletedAt] IS NULL AND [RetailerPriceObservationId] IS NULL) OR ([Status] = N'running' AND [LeaseToken] IS NOT NULL AND [LeaseExpiresAt] IS NOT NULL AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL AND [RetailerPriceObservationId] IS NULL) OR ([Status] = N'succeeded' AND [LeaseToken] IS NULL AND [LeaseExpiresAt] IS NULL AND [CompletedAt] IS NOT NULL AND [RetailerPriceObservationId] IS NOT NULL AND [FinalSourceUrl] IS NOT NULL AND [DiagnosticCode] IS NULL) OR ([Status] IN (N'failed', N'skipped') AND [LeaseToken] IS NULL AND [LeaseExpiresAt] IS NULL AND [CompletedAt] IS NOT NULL AND [RetailerPriceObservationId] IS NULL AND [DiagnosticCode] IS NOT NULL AND [DiagnosticMessage] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRunItems_RetailerListings_RetailerListingId",
                        column: x => x.RetailerListingId,
                        principalTable: "RetailerListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRunItems_RetailerPriceCollectionRuns_RunId",
                        column: x => x.RunId,
                        principalTable: "RetailerPriceCollectionRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RetailerPriceCollectionRunItems_RetailerPriceObservations_RetailerPriceObservationId",
                        column: x => x.RetailerPriceObservationId,
                        principalTable: "RetailerPriceObservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCatalogNodes_NumberId",
                table: "ActivityCatalogNodes",
                column: "NumberId",
                unique: true,
                filter: "[NumberId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCatalogNodes_ParentFolderId_NormalizedName",
                table: "ActivityCatalogNodes",
                columns: new[] { "ParentFolderId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCatalogNodes_ParentFolderId_SortOrder",
                table: "ActivityCatalogNodes",
                columns: new[] { "ParentFolderId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCatalogNodes_Status",
                table: "ActivityCatalogNodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductRequirements_ProductTypeId",
                table: "ActivityProductRequirements",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductRequirements_SectionId_SortOrder",
                table: "ActivityProductRequirements",
                columns: new[] { "SectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRequirementSections_ActivityId_SortOrder",
                table: "ActivityRequirementSections",
                columns: new[] { "ActivityId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserSite_UsersId",
                table: "ApplicationUserSite",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NumberId",
                table: "AspNetUsers",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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
                name: "IX_Assets_Classification",
                table: "Assets",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Classification_Name_NumberId",
                table: "Assets",
                columns: new[] { "Classification", "Name", "NumberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Condition",
                table: "Assets",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Manufacturer",
                table: "Assets",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ManufacturerSerialNumber",
                table: "Assets",
                column: "ManufacturerSerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Model",
                table: "Assets",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Name",
                table: "Assets",
                column: "Name");

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

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_NumberId",
                table: "Cameras",
                column: "NumberId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_SiteId",
                table: "Cameras",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NormalizedName",
                table: "Categories",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date",
                table: "Invoices",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubmittedFromSiteId",
                table: "Invoices",
                column: "SubmittedFromSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SupplierId",
                table: "Invoices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueAttachments_IssueId_Created",
                table: "IssueAttachments",
                columns: new[] { "IssueId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_EndDate",
                table: "Issues",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_NumberId",
                table: "Issues",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SiteId",
                table: "Issues",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_StartDate",
                table: "Issues",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Status",
                table: "Issues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IssueWorkers_AssignedWorkersId",
                table: "IssueWorkers",
                column: "AssignedWorkersId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivities_OfferId_SortOrder",
                table: "OfferActivities",
                columns: new[] { "OfferId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivities_OfferId_SourceActivityId",
                table: "OfferActivities",
                columns: new[] { "OfferId", "SourceActivityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivities_SourceActivityId",
                table: "OfferActivities",
                column: "SourceActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivitySections_OfferActivityId_SortOrder",
                table: "OfferActivitySections",
                columns: new[] { "OfferActivityId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivitySections_OfferActivityId_SourceSectionId",
                table: "OfferActivitySections",
                columns: new[] { "OfferActivityId", "SourceSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductLines_OfferId_ProductNumberId",
                table: "OfferProductLines",
                columns: new[] { "OfferId", "ProductNumberId" });

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
                name: "IX_OfferProductLines_ProductId",
                table: "OfferProductLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductPriceSelections_OfferProductLineId",
                table: "OfferProductPriceSelections",
                column: "OfferProductLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductPriceSelections_RetailerId",
                table: "OfferProductPriceSelections",
                column: "RetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductPriceSelections_RetailerListingId",
                table: "OfferProductPriceSelections",
                column: "RetailerListingId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferProductPriceSelections_RetailerPriceObservationId",
                table: "OfferProductPriceSelections",
                column: "RetailerPriceObservationId");

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
                name: "IX_OfferRetailerComparisons_OfferId_RetailerId",
                table: "OfferRetailerComparisons",
                columns: new[] { "OfferId", "RetailerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferRetailerComparisons_RetailerId",
                table: "OfferRetailerComparisons",
                column: "RetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_NumberId",
                table: "Offers",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SiteId",
                table: "Offers",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SiteId_Status_NumberId",
                table: "Offers",
                columns: new[] { "SiteId", "Status", "NumberId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonAddresses_PersonId",
                table: "PersonAddresses",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonBankAccounts_PersonId",
                table: "PersonBankAccounts",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_PersonId",
                table: "PersonContacts",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Egn",
                table: "Persons",
                column: "Egn");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Eik",
                table: "Persons",
                column: "Eik");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_NumberId",
                table: "Persons",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_SearchName",
                table: "Persons",
                column: "SearchName");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_SearchTaxIdentifier",
                table: "Persons",
                column: "SearchTaxIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NumberId",
                table: "Products",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductTypeId",
                table: "Products",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Title",
                table: "Products",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_CategoryId_NormalizedName",
                table: "ProductTypes",
                columns: new[] { "CategoryId", "NormalizedName" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionAllowedHosts_ExtractionProfileId_NormalizedHost",
                table: "RetailerExtractionAllowedHosts",
                columns: new[] { "ExtractionProfileId", "NormalizedHost" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionProfiles_CompanyPersonId_Version",
                table: "RetailerExtractionProfiles",
                columns: new[] { "CompanyPersonId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionProfiles_Id_LastSuccessfulTestRuleId",
                table: "RetailerExtractionProfiles",
                columns: new[] { "Id", "LastSuccessfulTestRuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionProfiles_OneActivePerCompanyPerson",
                table: "RetailerExtractionProfiles",
                columns: new[] { "CompanyPersonId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionProfiles_OneDraftPerCompanyPerson",
                table: "RetailerExtractionProfiles",
                columns: new[] { "CompanyPersonId", "Status" },
                unique: true,
                filter: "[Status] = N'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerExtractionRules_ExtractionProfileId_Priority",
                table: "RetailerExtractionRules",
                columns: new[] { "ExtractionProfileId", "Priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailerListings_ProductId_IsActive",
                table: "RetailerListings",
                columns: new[] { "ProductId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerListings_ProductId_RetailerId",
                table: "RetailerListings",
                columns: new[] { "ProductId", "RetailerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailerListings_RetailerId_IsActive",
                table: "RetailerListings",
                columns: new[] { "RetailerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRunItems_RetailerListingId",
                table: "RetailerPriceCollectionRunItems",
                column: "RetailerListingId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRunItems_RetailerPriceObservationId",
                table: "RetailerPriceCollectionRunItems",
                column: "RetailerPriceObservationId",
                unique: true,
                filter: "[RetailerPriceObservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRunItems_RunId_RetailerListingId",
                table: "RetailerPriceCollectionRunItems",
                columns: new[] { "RunId", "RetailerListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRunItems_Status_LeaseExpiresAt_Id",
                table: "RetailerPriceCollectionRunItems",
                columns: new[] { "Status", "LeaseExpiresAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRuns_CompanyPersonId_RequestedAt_Id",
                table: "RetailerPriceCollectionRuns",
                columns: new[] { "CompanyPersonId", "RequestedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRuns_ExtractionProfileId",
                table: "RetailerPriceCollectionRuns",
                column: "ExtractionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRuns_OfferId_RequestedAt_Id",
                table: "RetailerPriceCollectionRuns",
                columns: new[] { "OfferId", "RequestedAt", "Id" },
                descending: new[] { false, true, true },
                filter: "[OfferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceCollectionRuns_OneUnfinishedPerCompany",
                table: "RetailerPriceCollectionRuns",
                column: "CompanyPersonId",
                unique: true,
                filter: "[Status] IN (N'queued', N'running')");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceObservations_ExtractionProfileId_MatchedExtractionRuleId",
                table: "RetailerPriceObservations",
                columns: new[] { "ExtractionProfileId", "MatchedExtractionRuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_RetailerPriceObservations_RetailerListingId_ObservedAt_RecordedAt_Id",
                table: "RetailerPriceObservations",
                columns: new[] { "RetailerListingId", "ObservedAt", "RecordedAt", "Id" },
                descending: new[] { false, true, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_CompanyPersonId",
                table: "Retailers",
                column: "CompanyPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_IsActive",
                table: "Retailers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_NormalizedName",
                table: "Retailers",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_NormalizedWebsiteHost",
                table: "Retailers",
                column: "NormalizedWebsiteHost");

            migrationBuilder.CreateIndex(
                name: "IX_SitePayments_InvoiceId_SiteId",
                table: "SitePayments",
                columns: new[] { "InvoiceId", "SiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePayments_SiteId",
                table: "SitePayments",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_ManagerId",
                table: "Sites",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_PrimaryClientUserId",
                table: "Sites",
                column: "PrimaryClientUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferProductPriceSelections_RetailerPriceObservations_RetailerPriceObservationId",
                table: "OfferProductPriceSelections",
                column: "RetailerPriceObservationId",
                principalTable: "RetailerPriceObservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RetailerExtractionAllowedHosts_RetailerExtractionProfiles_ExtractionProfileId",
                table: "RetailerExtractionAllowedHosts",
                column: "ExtractionProfileId",
                principalTable: "RetailerExtractionProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RetailerExtractionProfiles_RetailerExtractionRules_Id_LastSuccessfulTestRuleId",
                table: "RetailerExtractionProfiles",
                columns: new[] { "Id", "LastSuccessfulTestRuleId" },
                principalTable: "RetailerExtractionRules",
                principalColumns: new[] { "ExtractionProfileId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RetailerExtractionProfiles_Persons_CompanyPersonId",
                table: "RetailerExtractionProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_RetailerExtractionRules_RetailerExtractionProfiles_ExtractionProfileId",
                table: "RetailerExtractionRules");

            migrationBuilder.DropTable(
                name: "ActivityProductRequirements");

            migrationBuilder.DropTable(
                name: "ApplicationUserSite");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssetEvents");

            migrationBuilder.DropTable(
                name: "Cameras");

            migrationBuilder.DropTable(
                name: "IssueAttachments");

            migrationBuilder.DropTable(
                name: "IssueWorkers");

            migrationBuilder.DropTable(
                name: "OfferProductPriceSelections");

            migrationBuilder.DropTable(
                name: "OfferRetailerComparisons");

            migrationBuilder.DropTable(
                name: "PersonAddresses");

            migrationBuilder.DropTable(
                name: "PersonBankAccounts");

            migrationBuilder.DropTable(
                name: "PersonContacts");

            migrationBuilder.DropTable(
                name: "ProposalActivitySections");

            migrationBuilder.DropTable(
                name: "ProposalDocuments");

            migrationBuilder.DropTable(
                name: "ProposalProductLines");

            migrationBuilder.DropTable(
                name: "RetailerExtractionAllowedHosts");

            migrationBuilder.DropTable(
                name: "RetailerPriceCollectionRunItems");

            migrationBuilder.DropTable(
                name: "RetailerPriceCollectionWorkerLeases");

            migrationBuilder.DropTable(
                name: "SiteFiles");

            migrationBuilder.DropTable(
                name: "SiteImages");

            migrationBuilder.DropTable(
                name: "SitePayments");

            migrationBuilder.DropTable(
                name: "SiteVideos");

            migrationBuilder.DropTable(
                name: "ActivityRequirementSections");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "OfferProductLines");

            migrationBuilder.DropTable(
                name: "ProposalActivities");

            migrationBuilder.DropTable(
                name: "RetailerPriceCollectionRuns");

            migrationBuilder.DropTable(
                name: "RetailerPriceObservations");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "OfferProductRequirements");

            migrationBuilder.DropTable(
                name: "Proposals");

            migrationBuilder.DropTable(
                name: "RetailerListings");

            migrationBuilder.DropTable(
                name: "OfferActivitySections");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Retailers");

            migrationBuilder.DropTable(
                name: "OfferActivities");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "ActivityCatalogNodes");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "RetailerExtractionProfiles");

            migrationBuilder.DropTable(
                name: "RetailerExtractionRules");

            migrationBuilder.DropSequence(
                name: "ActivityNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "ApplicationUserNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "AssetNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "CameraNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "InvoiceNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "IssueNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "OfferNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "PersonNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "ProductNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "ProposalNumberIds",
                schema: "dbo");

            migrationBuilder.DropSequence(
                name: "SiteNumberIds",
                schema: "dbo");
        }
    }
}
