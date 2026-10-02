using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Products.Commands;
using Application.Products.Queries;
using Application.RetailerListings;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class Products : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "products")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapPost(string.Empty, CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a product catalog entry");
        group.MapGet("/{productId:guid}", GetProduct)
            .WithName("GetProduct")
            .WithSummary("Get a product catalog entry by ID");
        group.MapPut("/{productId:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update a product catalog entry");
        group.MapGet(
                "/{productId:guid}/retailers/{retailerId:guid}/price-history",
                GetRetailerPriceHistory)
            .WithName("GetRetailerPriceHistory")
            .WithSummary("Get newest-first price history for a Product and Retailer")
            .Produces<RetailerPriceHistoryDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{productId:guid}/retailer-listings", GetRetailerListings)
            .WithName("GetProductRetailerListings")
            .WithSummary("Get paged Retailer listings for a Product")
            .Produces<PagedResult<RetailerListingDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        dashboardGroup.MapGet("/products", GetDashboardProducts)
            .WithName("GetDashboardProducts")
            .WithSummary("Get a filtered and paged product catalog");
        dashboardGroup.MapGet("/products/search", SearchDashboardProducts)
            .WithName("SearchDashboardProducts")
            .WithSummary("Search active products for assignment lookup");
    }

    private static async Task<IResult> CreateProduct(
        IMediator mediator,
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var productId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/products/{productId}", new { id = productId });
    }

    private static async Task<Ok<ProductDetailsDto>> GetProduct(
        IMediator mediator,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await mediator.Send(
            new ProductByIdQuery { ProductId = productId },
            cancellationToken);
        return TypedResults.Ok(product);
    }

    private static async Task<NoContent> UpdateProduct(
        IMediator mediator,
        Guid productId,
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = productId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<RetailerListingDto>>> GetRetailerListings(
        IMediator mediator,
        [AsParameters] ProductRetailerListingsQuery query,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(query, cancellationToken));

    private static async Task<Ok<PagedResult<ProductTableDto>>> GetDashboardProducts(
        IMediator mediator,
        [AsParameters] DashboardProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(products);
    }

    private static async Task<Ok<List<ProductLookupDto>>> SearchDashboardProducts(
        IMediator mediator,
        [AsParameters] ProductSearchQuery query,
        CancellationToken cancellationToken)
    {
        var products = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(products);
    }

    private static async Task<Ok<RetailerPriceHistoryDto>> GetRetailerPriceHistory(
        IMediator mediator,
        Guid productId,
        Guid retailerId,
        int pageIndex = 0,
        int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerPriceHistoryQuery
            {
                ProductId = productId,
                RetailerId = retailerId,
                PageIndex = pageIndex,
                PageSize = pageSize
            },
            cancellationToken));
}
