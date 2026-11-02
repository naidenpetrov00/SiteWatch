using System.Net;
using System.Net.Http.Json;
using Api.Tests.Infrastructure;
using Application.ProductCatalog;
using Application.SeedWork.Models;
using NSubstitute;

namespace Api.Tests.Endpoints.Products;

public sealed class ProductCatalogEndpointsTests
{
    [Fact]
    public async Task Category_and_product_type_routes_bind_their_authoritative_contracts()
    {
        await using var factory = new SiteWatchApiFactory();
        var categoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var productTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(categoryId);
        factory.Mediator.Send(Arg.Any<CreateProductTypeCommand>(), Arg.Any<CancellationToken>())
            .Returns(productTypeId);
        factory.Mediator.Send(Arg.Any<CategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<CategoryDto>([new(categoryId, "Adhesives")], 1, 1));
        factory.Mediator.Send(Arg.Any<ProductTypesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<ProductTypeDto>([new(productTypeId, "Tile Adhesive", categoryId, "Adhesives")], 1, 1));
        using var client = factory.CreateHttpsClient();

        var createCategory = await client.PostAsJsonAsync("/categories", new { name = "Adhesives" });
        var createType = await client.PostAsJsonAsync("/product-types", new { categoryId, name = "Tile Adhesive" });
        var categories = await client.GetAsync("/categories?searchTerm=adh&pageIndex=0&pageSize=20");
        var types = await client.GetAsync($"/product-types?categoryId={categoryId}&searchTerm=tile&pageIndex=0&pageSize=20");

        Assert.Equal(HttpStatusCode.Created, createCategory.StatusCode);
        Assert.Equal($"/categories/{categoryId}", createCategory.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Created, createType.StatusCode);
        Assert.Equal($"/product-types/{productTypeId}", createType.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, categories.StatusCode);
        Assert.Equal(HttpStatusCode.OK, types.StatusCode);
        await factory.Mediator.Received(1).Send(
            Arg.Is<CreateCategoryCommand>(command => command.Name == "Adhesives"),
            Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(
            Arg.Is<CreateProductTypeCommand>(command =>
                command.CategoryId == categoryId && command.Name == "Tile Adhesive"),
            Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(
            Arg.Is<ProductTypesQuery>(query =>
                query.CategoryId == categoryId && query.SearchTerm == "tile" && query.PageSize == 20),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Catalog_update_and_delete_routes_use_the_route_identifier()
    {
        await using var factory = new SiteWatchApiFactory();
        var categoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var productTypeId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        factory.Mediator.Send(Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<UpdateProductTypeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteProductTypeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var responses = await Task.WhenAll(
            client.PutAsJsonAsync($"/categories/{categoryId}", new { id = Guid.NewGuid(), name = "Coatings" }),
            client.PutAsJsonAsync($"/product-types/{productTypeId}", new { id = Guid.NewGuid(), categoryId, name = "Interior Paint" }),
            client.DeleteAsync($"/categories/{categoryId}"),
            client.DeleteAsync($"/product-types/{productTypeId}"));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        await factory.Mediator.Received(1).Send(
            Arg.Is<UpdateCategoryCommand>(command => command.Id == categoryId && command.Name == "Coatings"),
            Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(
            Arg.Is<UpdateProductTypeCommand>(command =>
                command.Id == productTypeId && command.CategoryId == categoryId && command.Name == "Interior Paint"),
            Arg.Any<CancellationToken>());
    }
}
