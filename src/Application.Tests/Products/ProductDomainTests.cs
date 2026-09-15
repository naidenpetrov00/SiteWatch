using Application.Products.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Tests.Products;

public sealed class ProductDomainTests
{
    [Fact]
    public void Product_normalizes_details_and_builds_a_search_identity_from_its_effective_phrase()
    {
        var product = Product.Create(
            "  Cordless   Drill  ", "  Brushless   drill ", " Bosch ", " GSB 18V ",
            1, ProductPackageUnit.Piece, ProductCategory.ToolsEquipment, ProductStatus.Active,
            ProductSearchConfiguration.Create(null));

        Assert.Equal("Cordless Drill", product.Title);
        Assert.Equal("Brushless drill", product.Description);
        Assert.Equal("Bosch", product.Brand);
        Assert.Equal("GSB 18V", product.Model);
        Assert.Equal("Cordless Drill Bosch GSB 18V", product.EffectivePrimarySearchPhrase);
        Assert.Equal("CORDLESS DRILL TOOLS EQUIPMENT BOSCH GSB 18V CORDLESS DRILL BOSCH GSB 18V", product.SearchIdentity);
    }

    [Fact]
    public void Product_uses_a_custom_primary_phrase_and_exposes_its_editable_details()
    {
        var configuration = ProductSearchConfiguration.Create("  Bosch drill  ", [" Drill Driver "]);
        var product = Product.Create(
            "Cordless Drill", null, "Bosch", "GSB", null, null,
            ProductCategory.ToolsEquipment, ProductStatus.Unavailable, configuration);

        var details = ProductDetailsDto.From(product);

        Assert.Equal("Bosch drill", details.PrimarySearchPhrase);
        Assert.Equal("Bosch drill", details.EffectivePrimarySearchPhrase);
        Assert.Equal(["Drill Driver"], details.AlternativeSearchPhrases);
        Assert.Equal("tools-equipment", details.Category);
        Assert.Equal("Unavailable", details.Status);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Product_rejects_an_incomplete_package_pair(bool hasQuantity, bool hasUnit)
    {
        var quantity = hasQuantity ? 1m : (decimal?)null;
        var unit = hasUnit ? ProductPackageUnit.Piece : (ProductPackageUnit?)null;

        Assert.Throws<ArgumentException>(() => Product.Create(
            "Drill", null, null, null, quantity, unit, ProductCategory.ToolsEquipment,
            ProductStatus.Active, ProductSearchConfiguration.Create(null)));
    }

    [Fact]
    public void Product_rejects_a_non_positive_package_quantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(
            "Drill", null, null, null, 0, ProductPackageUnit.Piece,
            ProductCategory.ToolsEquipment, ProductStatus.Active, ProductSearchConfiguration.Create(null)));
    }

    [Fact]
    public void Search_configuration_normalizes_deduplicates_and_round_trips_storage()
    {
        var configuration = ProductSearchConfiguration.Create(
            "  Bosch   Drill ", [" GSB 18V ", "gsb 18v", ""], [" brushless "], [" used "]);

        var restored = ProductSearchConfiguration.FromStorageValue(configuration.ToStorageValue());

        Assert.Equal("Bosch Drill", restored.PrimarySearchPhrase);
        Assert.Equal(["GSB 18V"], restored.AlternativeSearchPhrases);
        Assert.Equal(["brushless"], restored.RequiredKeywords);
        Assert.Equal(["used"], restored.ExcludedKeywords);
        Assert.Equal(configuration, restored);
    }

    [Fact]
    public void Search_configuration_rejects_more_than_the_supported_number_of_entries()
    {
        var entries = Enumerable.Range(1, ProductSearchConfiguration.MaxCollectionCount + 1)
            .Select(index => $"term {index}");

        Assert.Throws<ArgumentException>(() => ProductSearchConfiguration.Create(null, entries));
    }

    [Theory]
    [InlineData(" tools-equipment ", ProductCategory.ToolsEquipment)]
    [InlineData("OTHER", ProductCategory.Other)]
    public void Product_category_codes_parse_case_insensitively(string value, ProductCategory expected)
    {
        Assert.True(ProductCategoryCodes.TryParse(value, out var category));
        Assert.Equal(expected, category);
    }

    [Theory]
    [InlineData(" kg ", ProductPackageUnit.Kilogram)]
    [InlineData("M3", ProductPackageUnit.CubicMeter)]
    public void Product_package_unit_codes_parse_case_insensitively(string value, ProductPackageUnit expected)
    {
        Assert.True(ProductPackageUnitCodes.TryParse(value, out var unit));
        Assert.Equal(expected, unit);
    }
}
