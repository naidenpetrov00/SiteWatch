using Application.Products.Commands;
using Application.Products.Queries;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Products;

public sealed class ProductApplicationTests
{
    [Fact]
    public async Task Create_product_validator_accepts_a_complete_supported_request()
    {
        var result = await new CreateProductValidator().ValidateAsync(ValidCreate());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("unknown", "Active", "piece", "Category")]
    [InlineData("tools-equipment", "Retired", "piece", "Status")]
    [InlineData("tools-equipment", "Active", "crate", "PackageUnit")]
    public async Task Create_product_validator_rejects_unsupported_catalog_codes(
        string category, string status, string packageUnit, string property)
    {
        var result = await new CreateProductValidator().ValidateAsync(ValidCreate() with
        {
            Category = category, Status = status, PackageUnit = packageUnit
        });

        Assert.Contains(result.Errors, error => error.PropertyName == property);
    }

    [Fact]
    public async Task Create_product_validator_requires_package_quantity_and_unit_together()
    {
        var result = await new CreateProductValidator().ValidateAsync(ValidCreate() with { PackageUnit = null });

        Assert.Contains(result.Errors, error => error.ErrorMessage == "PackageQuantity and PackageUnit must be supplied together.");
    }

    [Fact]
    public async Task Create_product_validator_rejects_blank_or_oversized_search_entries()
    {
        var result = await new CreateProductValidator().ValidateAsync(ValidCreate() with
        {
            AlternativeSearchPhrases = ["", new string('x', 201)]
        });

        Assert.Contains(result.Errors, error => error.PropertyName == "AlternativeSearchPhrases[0]");
        Assert.Contains(result.Errors, error => error.PropertyName == "AlternativeSearchPhrases[1]");
    }

    [Fact]
    public async Task Update_product_validator_requires_an_identifier()
    {
        var result = await new UpdateProductValidator().ValidateAsync(new UpdateProductCommand
        {
            Title = "Drill", Category = "tools-equipment", Status = "Active"
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProductCommand.Id));
    }

    [Theory]
    [InlineData(-1, 50, false)]
    [InlineData(0, 0, false)]
    [InlineData(0, 50, true)]
    public async Task Dashboard_products_validator_enforces_page_boundaries(int pageIndex, int pageSize, bool isValid)
    {
        var result = await new DashboardProductsQueryValidator().ValidateAsync(new DashboardProductsQuery
        {
            PageIndex = pageIndex, PageSize = pageSize
        });

        Assert.Equal(isValid, result.IsValid);
    }

    [Fact]
    public async Task Dashboard_products_validator_rejects_invalid_filters_and_sorting()
    {
        var result = await new DashboardProductsQueryValidator().ValidateAsync(new DashboardProductsQuery
        {
            NumberId = "not-a-number", Id = "not-a-guid", PackageQuantity = "one",
            Category = "invalid", PackageUnit = "crate", Status = "Retired",
            SortActive = "unknown", SortDirection = "sideways"
        });

        Assert.Equal(8, result.Errors.Count);
    }

    [Fact]
    public async Task Product_search_validator_enforces_the_search_term_limit()
    {
        var validator = new ProductSearchQueryValidator();

        Assert.True((await validator.ValidateAsync(new ProductSearchQuery { SearchTerm = new string('x', 200) })).IsValid);
        Assert.Contains((await validator.ValidateAsync(new ProductSearchQuery { SearchTerm = new string('x', 201) })).Errors,
            error => error.PropertyName == nameof(ProductSearchQuery.SearchTerm));
    }

    [Fact]
    public async Task Create_and_update_handlers_delegate_the_original_request_to_the_product_service()
    {
        var products = Substitute.For<IProductService>();
        var createdId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var create = ValidCreate();
        var update = new UpdateProductCommand
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Title = "Updated drill",
            Category = "tools-equipment", Status = "Active"
        };
        products.CreateAsync(create, CancellationToken.None).Returns(createdId);

        var result = await new CreateProductHandler(products).Handle(create, CancellationToken.None);
        await new UpdateProductHandler(products).Handle(update, CancellationToken.None);

        Assert.Equal(createdId, result);
        await products.Received(1).CreateAsync(create, CancellationToken.None);
        await products.Received(1).UpdateAsync(update, CancellationToken.None);
    }

    private static CreateProductCommand ValidCreate() => new()
    {
        Title = "Cordless Drill", Category = "tools-equipment", Status = "Active",
        PackageQuantity = 1, PackageUnit = "piece", AlternativeSearchPhrases = [],
        RequiredKeywords = [], ExcludedKeywords = []
    };
}
