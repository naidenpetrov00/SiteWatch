using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.Products.Commands;
using Application.Products.Queries;
using Application.SeedWork.Models;
using NSubstitute;

namespace Api.Tests.Endpoints.Products;

public sealed class ProductEndpointsTests
{
    [Fact]
    public async Task Create_product_binds_the_request_and_returns_the_created_resource()
    {
        await using var factory = new SiteWatchApiFactory();
        CreateProductCommand? captured = null;
        var productId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        factory.Mediator.Send(Arg.Do<CreateProductCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(productId);
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/products", ProductRequest());
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/products/{productId}", response.Headers.Location?.OriginalString);
        Assert.Equal(productId, payload.GetProperty("id").GetGuid());
        Assert.Equal("Cordless Drill", captured?.Title);
        Assert.Equal("tools-equipment", captured?.Category);
    }

    [Fact]
    public async Task Product_reads_return_the_detail_table_and_lookup_contracts()
    {
        await using var factory = new SiteWatchApiFactory();
        var productId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<ProductByIdQuery>(), Arg.Any<CancellationToken>()).Returns(ProductDetails(productId));
        factory.Mediator.Send(Arg.Any<DashboardProductsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<ProductTableDto>([ProductTable(productId)], 1, 3));
        factory.Mediator.Send(Arg.Any<ProductSearchQuery>(), Arg.Any<CancellationToken>())
            .Returns([new ProductLookupDto { Id = productId, NumberId = 7, Title = "Cordless Drill", Category = "tools-equipment" }]);
        using var client = factory.CreateHttpsClient();

        var detail = await client.GetAsync($"/products/{productId}");
        var table = await client.GetAsync("/dashboard/products?pageIndex=1&pageSize=50&title=drill&sortActive=title&sortDirection=asc");
        var lookup = await client.GetAsync("/dashboard/products/search?searchTerm=drill");

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Equal("Cordless Drill", (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.OK, table.StatusCode);
        Assert.Equal(3, (await table.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("totalCount").GetInt32());
        Assert.Equal(HttpStatusCode.OK, lookup.StatusCode);
        Assert.Equal(7, (await lookup.Content.ReadFromJsonAsync<JsonElement>())[0].GetProperty("numberId").GetInt32());
        await factory.Mediator.Received(1).Send(Arg.Is<ProductByIdQuery>(query => query.ProductId == productId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<DashboardProductsQuery>(query => query.PageIndex == 1 && query.Title == "drill" && query.SortActive == "title"), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<ProductSearchQuery>(query => query.SearchTerm == "drill"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_product_uses_the_route_identifier_over_the_request_body()
    {
        await using var factory = new SiteWatchApiFactory();
        UpdateProductCommand? captured = null;
        var routeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        factory.Mediator.Send(Arg.Do<UpdateProductCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var response = await client.PutAsJsonAsync($"/products/{routeId}", new
        {
            id = Guid.NewGuid(), title = "Cordless Drill", category = "tools-equipment", status = "Active"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(routeId, captured?.Id);
    }

    private static object ProductRequest() => new
    {
        title = "Cordless Drill", description = "Brushless drill", brand = "Bosch", model = "GSB 18V",
        packageQuantity = 1, packageUnit = "piece", category = "tools-equipment", status = "Active",
        primarySearchPhrase = (string?)null, alternativeSearchPhrases = Array.Empty<string>(),
        requiredKeywords = Array.Empty<string>(), excludedKeywords = Array.Empty<string>()
    };

    private static ProductDetailsDto ProductDetails(Guid id) => new()
    {
        Id = id, NumberId = 7, Title = "Cordless Drill", Category = "tools-equipment", Status = "Active",
        EffectivePrimarySearchPhrase = "Cordless Drill"
    };

    private static ProductTableDto ProductTable(Guid id) => new()
    {
        Id = id, NumberId = 7, Title = "Cordless Drill", Category = "tools-equipment", Status = "Active"
    };
}
