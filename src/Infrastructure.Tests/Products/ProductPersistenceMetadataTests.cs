using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Infrastructure.Tests.Products;

public sealed class ProductPersistenceMetadataTests
{
    [Fact]
    public void Product_mapping_preserves_required_columns_precision_indexes_and_generated_number_ids()
    {
        using var dbContext = CreateDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(Product))!;

        var title = entity.FindProperty(nameof(Product.Title))!;
        var quantity = entity.FindProperty(nameof(Product.PackageQuantity))!;
        var numberId = entity.FindProperty(nameof(Product.NumberId))!;

        Assert.False(title.IsNullable);
        Assert.Equal(200, title.GetMaxLength());
        Assert.Equal(18, quantity.GetPrecision());
        Assert.Equal(4, quantity.GetScale());
        Assert.Equal("NEXT VALUE FOR [dbo].[ProductNumberIds]", numberId.GetDefaultValueSql());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Single().Name == nameof(Product.NumberId));
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Single().Name == nameof(Product.Title));
    }

    [Fact]
    public void Product_mapping_round_trips_package_unit_and_search_configuration_converters_and_requires_a_product_type()
    {
        using var dbContext = CreateDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(Product))!;
        var packageUnit = entity.FindProperty(nameof(Product.PackageUnit))!.GetValueConverter()!;
        var searchConfiguration = entity.FindProperty(nameof(Product.SearchConfiguration))!.GetValueConverter()!;
        var configuration = ProductSearchConfiguration.Create("Bosch Drill", ["GSB 18V"], ["brushless"], ["used"]);

        Assert.Equal("kg", packageUnit.ConvertToProvider(ProductPackageUnit.Kilogram));
        Assert.Equal(ProductPackageUnit.Piece, packageUnit.ConvertFromProvider("pieces"));
        Assert.NotNull(entity.FindProperty(nameof(Product.ProductTypeId)));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ProductType)
            && foreignKey.Properties.Single().Name == nameof(Product.ProductTypeId)
            && foreignKey.DeleteBehavior == DeleteBehavior.Restrict);

        var stored = Assert.IsType<string>(searchConfiguration.ConvertToProvider(configuration));
        var restored = Assert.IsType<ProductSearchConfiguration>(searchConfiguration.ConvertFromProvider(stored));
        Assert.Equal(configuration, restored);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SiteWatchProductModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
