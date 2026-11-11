using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBrandCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductBrandId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBrands", x => x.Id);
                });
            migrationBuilder.Sql(
    """
    CREATE TABLE #ProductBrandBackfill
    (
        ProductId uniqueidentifier NOT NULL,
        BrandName nvarchar(100) NOT NULL
    );

    INSERT INTO #ProductBrandBackfill (ProductId, BrandName)
    SELECT
        Id,
        LTRIM(RTRIM(
            REPLACE(
                REPLACE(
                    REPLACE(
                        REPLACE(Brand, CHAR(9), N' '),
                        CHAR(10), N' '
                    ),
                    CHAR(13), N' '
                ),
                NCHAR(160), N' '
            )
        ))
    FROM Products
    WHERE Brand IS NOT NULL;

    WHILE EXISTS
    (
        SELECT 1
        FROM #ProductBrandBackfill
        WHERE BrandName LIKE N'%  %'
    )
    BEGIN
        UPDATE #ProductBrandBackfill
        SET BrandName = REPLACE(BrandName, N'  ', N' ');
    END;

    DELETE FROM #ProductBrandBackfill
    WHERE BrandName = N'';

    DECLARE @MigrationTimestamp datetimeoffset = SYSDATETIMEOFFSET();

    ;WITH BrandCandidates AS
    (
        SELECT
            BrandName,
            UPPER(BrandName) AS NormalizedName,
            COUNT_BIG(*) AS UsageCount
        FROM #ProductBrandBackfill
        GROUP BY BrandName, UPPER(BrandName)
    ),
    PreferredBrands AS
    (
        SELECT
            BrandName,
            NormalizedName,
            ROW_NUMBER() OVER
            (
                PARTITION BY NormalizedName
                ORDER BY UsageCount DESC, BrandName
            ) AS Preference
        FROM BrandCandidates
    )
    INSERT INTO ProductBrands
    (
        Id,
        Name,
        NormalizedName,
        Created,
        CreatedBy,
        LastModified,
        LastModifiedBy
    )
    SELECT
        NEWID(),
        BrandName,
        NormalizedName,
        @MigrationTimestamp,
        N'Migration',
        @MigrationTimestamp,
        N'Migration'
    FROM PreferredBrands
    WHERE Preference = 1;

    UPDATE Products
    SET ProductBrandId = ProductBrands.Id
    FROM Products
    INNER JOIN #ProductBrandBackfill
        ON #ProductBrandBackfill.ProductId = Products.Id
    INNER JOIN ProductBrands
        ON ProductBrands.NormalizedName =
           UPPER(#ProductBrandBackfill.BrandName);

    DROP TABLE #ProductBrandBackfill;
    """);
            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductBrandId",
                table: "Products",
                column: "ProductBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrands_NormalizedName",
                table: "ProductBrands",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductBrands_ProductBrandId",
                table: "Products",
                column: "ProductBrandId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductBrands_ProductBrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductBrands");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductBrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductBrandId",
                table: "Products");
        }
    }
}
