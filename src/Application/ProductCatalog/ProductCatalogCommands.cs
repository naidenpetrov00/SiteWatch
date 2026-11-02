using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.ProductCatalog;

/// <summary>Creates a Category.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateCategoryCommand(string Name) : IRequest<Guid>;

/// <summary>Updates a Category name.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateCategoryCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>Deletes an unreferenced Category.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteCategoryCommand(Guid Id) : IRequest;

/// <summary>Creates a Product Type within a Category.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateProductTypeCommand(Guid CategoryId, string Name) : IRequest<Guid>;

/// <summary>Updates a Product Type name and Category.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateProductTypeCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>Deletes an unreferenced Product Type.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteProductTypeCommand(Guid Id) : IRequest;

public sealed class ProductCatalogCommandHandler(IProductCatalogService service)
    : IRequestHandler<CreateCategoryCommand, Guid>,
      IRequestHandler<UpdateCategoryCommand>,
      IRequestHandler<DeleteCategoryCommand>,
      IRequestHandler<CreateProductTypeCommand, Guid>,
      IRequestHandler<UpdateProductTypeCommand>,
      IRequestHandler<DeleteProductTypeCommand>
{
    public Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken) =>
        service.CreateCategoryAsync(request.Name, cancellationToken);

    public Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken) =>
        service.UpdateCategoryAsync(request.Id, request.Name, cancellationToken);

    public Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken) =>
        service.DeleteCategoryAsync(request.Id, cancellationToken);

    public Task<Guid> Handle(CreateProductTypeCommand request, CancellationToken cancellationToken) =>
        service.CreateProductTypeAsync(request.CategoryId, request.Name, cancellationToken);

    public Task Handle(UpdateProductTypeCommand request, CancellationToken cancellationToken) =>
        service.UpdateProductTypeAsync(
            request.Id,
            request.CategoryId,
            request.Name,
            cancellationToken);

    public Task Handle(DeleteProductTypeCommand request, CancellationToken cancellationToken) =>
        service.DeleteProductTypeAsync(request.Id, cancellationToken);
}

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator() => RuleFor(request => request.Name)
        .NotEmpty().MaximumLength(Category.MaxNameLength);
}

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
    }
}

public sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator() => RuleFor(request => request.Id).NotEmpty();
}

public sealed class CreateProductTypeValidator : AbstractValidator<CreateProductTypeCommand>
{
    public CreateProductTypeValidator()
    {
        RuleFor(request => request.CategoryId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(ProductType.MaxNameLength);
    }
}

public sealed class UpdateProductTypeValidator : AbstractValidator<UpdateProductTypeCommand>
{
    public UpdateProductTypeValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.CategoryId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(ProductType.MaxNameLength);
    }
}

public sealed class DeleteProductTypeValidator : AbstractValidator<DeleteProductTypeCommand>
{
    public DeleteProductTypeValidator() => RuleFor(request => request.Id).NotEmpty();
}
