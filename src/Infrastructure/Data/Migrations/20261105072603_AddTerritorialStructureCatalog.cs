using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTerritorialStructureCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "District",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Municipality",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "Settlement",
                table: "PersonAddresses");

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "PersonAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MunicipalityId",
                table: "PersonAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuarterId",
                table: "PersonAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SettlementId",
                table: "PersonAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quarters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quarters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quarters_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonAddresses_DistrictId",
                table: "PersonAddresses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAddresses_MunicipalityId",
                table: "PersonAddresses",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAddresses_QuarterId",
                table: "PersonAddresses",
                column: "QuarterId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAddresses_SettlementId",
                table: "PersonAddresses",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_SettlementId_NormalizedName",
                table: "Districts",
                columns: new[] { "SettlementId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_NormalizedName",
                table: "Municipalities",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quarters_SettlementId_NormalizedName",
                table: "Quarters",
                columns: new[] { "SettlementId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_MunicipalityId_NormalizedName",
                table: "Settlements",
                columns: new[] { "MunicipalityId", "NormalizedName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddresses_Districts_DistrictId",
                table: "PersonAddresses",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddresses_Municipalities_MunicipalityId",
                table: "PersonAddresses",
                column: "MunicipalityId",
                principalTable: "Municipalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddresses_Quarters_QuarterId",
                table: "PersonAddresses",
                column: "QuarterId",
                principalTable: "Quarters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddresses_Settlements_SettlementId",
                table: "PersonAddresses",
                column: "SettlementId",
                principalTable: "Settlements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddresses_Districts_DistrictId",
                table: "PersonAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddresses_Municipalities_MunicipalityId",
                table: "PersonAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddresses_Quarters_QuarterId",
                table: "PersonAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddresses_Settlements_SettlementId",
                table: "PersonAddresses");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Quarters");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_PersonAddresses_DistrictId",
                table: "PersonAddresses");

            migrationBuilder.DropIndex(
                name: "IX_PersonAddresses_MunicipalityId",
                table: "PersonAddresses");

            migrationBuilder.DropIndex(
                name: "IX_PersonAddresses_QuarterId",
                table: "PersonAddresses");

            migrationBuilder.DropIndex(
                name: "IX_PersonAddresses_SettlementId",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "MunicipalityId",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "QuarterId",
                table: "PersonAddresses");

            migrationBuilder.DropColumn(
                name: "SettlementId",
                table: "PersonAddresses");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipality",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Quarter",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settlement",
                table: "PersonAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
