using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.ProductCatalog;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class ProductCatalog : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var categories = app.MapGroupCustom(customGroupName: "categories")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var productTypes = app.MapGroupCustom(customGroupName: "product-types")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        categories.MapPost(string.Empty, CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Create an administrator-managed Category")
            .Produces<CategoryCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);
        categories.MapGet(string.Empty, GetCategories)
            .WithName("GetCategories")
            .WithSummary("Get a paged list of Categories")
            .Produces<PagedResult<CategoryDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        categories.MapGet("/{categoryId:guid}", GetCategory)
            .WithName("GetCategory")
            .WithSummary("Get a Category by ID")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        categories.MapPut("/{categoryId:guid}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Update a Category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        categories.MapDelete("/{categoryId:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Delete an unreferenced Category")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        productTypes.MapPost(string.Empty, CreateProductType)
            .WithName("CreateProductType")
            .WithSummary("Create a Product Type within a Category")
            .Produces<ProductTypeCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        productTypes.MapGet(string.Empty, GetProductTypes)
            .WithName("GetProductTypes")
            .WithSummary("Get a paged list of Product Types")
            .Produces<PagedResult<ProductTypeDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        productTypes.MapGet("/{productTypeId:guid}", GetProductType)
            .WithName("GetProductType")
            .WithSummary("Get a Product Type by ID")
            .Produces<ProductTypeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        productTypes.MapPut("/{productTypeId:guid}", UpdateProductType)
            .WithName("UpdateProductType")
            .WithSummary("Update a Product Type")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        productTypes.MapDelete("/{productTypeId:guid}", DeleteProductType)
            .WithName("DeleteProductType")
            .WithSummary("Delete an unreferenced Product Type")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Created<CategoryCreatedResponse>> CreateCategory(
        IMediator mediator,
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/categories/{id}", new CategoryCreatedResponse(id));
    }

    private static async Task<Ok<PagedResult<CategoryDto>>> GetCategories(
        IMediator mediator,
        [AsParameters] CategoriesQuery query,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(query, cancellationToken));

    private static async Task<Ok<CategoryDto>> GetCategory(
        IMediator mediator,
        Guid categoryId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(new CategoryByIdQuery(categoryId), cancellationToken));

    private static async Task<NoContent> UpdateCategory(
        IMediator mediator,
        Guid categoryId,
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = categoryId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteCategory(
        IMediator mediator,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Created<ProductTypeCreatedResponse>> CreateProductType(
        IMediator mediator,
        CreateProductTypeCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/product-types/{id}", new ProductTypeCreatedResponse(id));
    }

    private static async Task<Ok<PagedResult<ProductTypeDto>>> GetProductTypes(
        IMediator mediator,
        [AsParameters] ProductTypesQuery query,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(query, cancellationToken));

    private static async Task<Ok<ProductTypeDto>> GetProductType(
        IMediator mediator,
        Guid productTypeId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new ProductTypeByIdQuery(productTypeId),
            cancellationToken));

    private static async Task<NoContent> UpdateProductType(
        IMediator mediator,
        Guid productTypeId,
        UpdateProductTypeCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = productTypeId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteProductType(
        IMediator mediator,
        Guid productTypeId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteProductTypeCommand(productTypeId), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Represents the identifier of a newly created Category.</summary>
    public sealed record CategoryCreatedResponse(Guid Id);

    /// <summary>Represents the identifier of a newly created Product Type.</summary>
    public sealed record ProductTypeCreatedResponse(Guid Id);
}
