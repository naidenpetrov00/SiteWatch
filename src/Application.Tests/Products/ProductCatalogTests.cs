using Application.ProductCatalog;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using NSubstitute;

namespace Application.Tests.Products;

public sealed class ProductCatalogTests
{
    [Fact]
    public void Category_and_product_type_normalize_names_and_product_type_can_move_categories()
    {
        var adhesives = Category.Create("  Tile   adhesives ");
        var coatings = Category.Create("Coatings");
        var type = ProductType.Create(adhesives, "  Flexible   Adhesive ");

        type.Update(coatings, "  Interior   Primer ");

        Assert.Equal("Tile adhesives", adhesives.Name);
        Assert.Equal("TILE ADHESIVES", adhesives.NormalizedName);
        Assert.Equal(coatings.Id, type.CategoryId);
        Assert.Equal("Interior Primer", type.Name);
        Assert.Equal("INTERIOR PRIMER", type.NormalizedName);
    }

    [Fact]
    public async Task Product_catalog_validators_require_names_and_category_identifiers()
    {
        var category = await new CreateCategoryValidator().ValidateAsync(new CreateCategoryCommand(" "));
        var type = await new CreateProductTypeValidator().ValidateAsync(
            new CreateProductTypeCommand(Guid.Empty, " "));
        var update = await new UpdateProductTypeValidator().ValidateAsync(
            new UpdateProductTypeCommand { Id = Guid.Empty, CategoryId = Guid.Empty, Name = " " });

        Assert.False(category.IsValid);
        Assert.False(type.IsValid);
        Assert.False(update.IsValid);
    }

    [Fact]
    public async Task Product_catalog_handlers_delegate_the_requested_mutations()
    {
        var service = Substitute.For<IProductCatalogService>();
        var categoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var productTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        service.CreateCategoryAsync("Coatings", CancellationToken.None).Returns(categoryId);
        service.CreateProductTypeAsync(categoryId, "Interior Paint", CancellationToken.None)
            .Returns(productTypeId);
        var handler = new ProductCatalogCommandHandler(service);

        Assert.Equal(categoryId, await handler.Handle(
            new CreateCategoryCommand("Coatings"), CancellationToken.None));
        Assert.Equal(productTypeId, await handler.Handle(
            new CreateProductTypeCommand(categoryId, "Interior Paint"), CancellationToken.None));
        await handler.Handle(
            new UpdateProductTypeCommand
            {
                Id = productTypeId,
                CategoryId = categoryId,
                Name = "Exterior Paint"
            },
            CancellationToken.None);

        await service.Received(1).UpdateProductTypeAsync(
            productTypeId,
            categoryId,
            "Exterior Paint",
            CancellationToken.None);
    }
}
