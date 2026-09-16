using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.ActivityCatalog.Commands;

/// <summary>Adds an active Product Catalog item to a requirement section.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateActivityProductRequirementCommand : IRequest<Guid>
{
    public Guid ActivityId { get; set; }
    public Guid SectionId { get; set; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
    public bool IsRequired { get; init; }
    public string QuantityBehavior { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public sealed class CreateActivityProductRequirementHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<CreateActivityProductRequirementCommand, Guid>
{
    public Task<Guid> Handle(
        CreateActivityProductRequirementCommand request,
        CancellationToken cancellationToken) =>
        requirementService.CreateProductRequirementAsync(
            request.ActivityId,
            request.SectionId,
            request.ProductId,
            request.Quantity,
            request.IsRequired,
            ProductQuantityBehaviorParser.Parse(request.QuantityBehavior),
            request.Notes,
            cancellationToken);
}

/// <summary>Updates an existing activity product requirement.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateActivityProductRequirementCommand : IRequest
{
    public Guid ActivityId { get; set; }
    public Guid SectionId { get; set; }
    public Guid RequirementId { get; set; }
    public decimal Quantity { get; init; }
    public bool IsRequired { get; init; }
    public string QuantityBehavior { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public sealed class UpdateActivityProductRequirementHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<UpdateActivityProductRequirementCommand>
{
    public Task Handle(
        UpdateActivityProductRequirementCommand request,
        CancellationToken cancellationToken) =>
        requirementService.UpdateProductRequirementAsync(
            request.ActivityId,
            request.SectionId,
            request.RequirementId,
            request.Quantity,
            request.IsRequired,
            ProductQuantityBehaviorParser.Parse(request.QuantityBehavior),
            request.Notes,
            cancellationToken);
}

/// <summary>Moves a product requirement within its section.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record MoveActivityProductRequirementCommand : IRequest
{
    public Guid ActivityId { get; set; }
    public Guid SectionId { get; set; }
    public Guid RequirementId { get; set; }
    public int TargetIndex { get; init; }
}

public sealed class MoveActivityProductRequirementHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<MoveActivityProductRequirementCommand>
{
    public Task Handle(
        MoveActivityProductRequirementCommand request,
        CancellationToken cancellationToken) =>
        requirementService.MoveProductRequirementAsync(
            request.ActivityId,
            request.SectionId,
            request.RequirementId,
            request.TargetIndex,
            cancellationToken);
}

/// <summary>Removes a Product Catalog relationship from a requirement section.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteActivityProductRequirementCommand(
    Guid ActivityId,
    Guid SectionId,
    Guid RequirementId) : IRequest;

public sealed class DeleteActivityProductRequirementHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<DeleteActivityProductRequirementCommand>
{
    public Task Handle(
        DeleteActivityProductRequirementCommand request,
        CancellationToken cancellationToken) =>
        requirementService.DeleteProductRequirementAsync(
            request.ActivityId,
            request.SectionId,
            request.RequirementId,
            cancellationToken);
}

public sealed class CreateActivityProductRequirementValidator
    : AbstractValidator<CreateActivityProductRequirementCommand>
{
    public CreateActivityProductRequirementValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.ProductId).NotEmpty();
        RuleFor(request => request.Quantity)
            .GreaterThan(0m)
            .PrecisionScale(18, 4, false);
        RuleFor(request => request.QuantityBehavior)
            .NotEmpty()
            .Must(value => ProductQuantityBehaviorCodes.TryParse(value, out _))
            .WithMessage("Unsupported product quantity behavior.");
        RuleFor(request => request.Notes)
            .MaximumLength(ActivityProductRequirement.MaxNotesLength);
    }
}

public sealed class UpdateActivityProductRequirementValidator
    : AbstractValidator<UpdateActivityProductRequirementCommand>
{
    public UpdateActivityProductRequirementValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.RequirementId).NotEmpty();
        RuleFor(request => request.Quantity)
            .GreaterThan(0m)
            .PrecisionScale(18, 4, false);
        RuleFor(request => request.QuantityBehavior)
            .NotEmpty()
            .Must(value => ProductQuantityBehaviorCodes.TryParse(value, out _))
            .WithMessage("Unsupported product quantity behavior.");
        RuleFor(request => request.Notes)
            .MaximumLength(ActivityProductRequirement.MaxNotesLength);
    }
}

public sealed class MoveActivityProductRequirementValidator
    : AbstractValidator<MoveActivityProductRequirementCommand>
{
    public MoveActivityProductRequirementValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.RequirementId).NotEmpty();
        RuleFor(request => request.TargetIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class DeleteActivityProductRequirementValidator
    : AbstractValidator<DeleteActivityProductRequirementCommand>
{
    public DeleteActivityProductRequirementValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.RequirementId).NotEmpty();
    }
}

internal static class ProductQuantityBehaviorParser
{
    public static ProductQuantityBehavior Parse(string value) =>
        ProductQuantityBehaviorCodes.TryParse(value, out var behavior)
            ? behavior
            : throw new ArgumentException(
                $"Unsupported product quantity behavior '{value}'.",
                nameof(value));
}
