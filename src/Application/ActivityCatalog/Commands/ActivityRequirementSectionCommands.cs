using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.ActivityCatalog.Commands;

/// <summary>Creates a measurement-based product requirement section.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateActivityRequirementSectionCommand : IRequest<Guid>
{
    public Guid ActivityId { get; set; }
    public string? Name { get; init; }
    public decimal BasisQuantity { get; init; }
    public string MeasurementUnit { get; init; } = string.Empty;
}

public sealed class CreateActivityRequirementSectionHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<CreateActivityRequirementSectionCommand, Guid>
{
    public Task<Guid> Handle(
        CreateActivityRequirementSectionCommand request,
        CancellationToken cancellationToken) =>
        requirementService.CreateSectionAsync(
            request.ActivityId,
            request.Name,
            request.BasisQuantity,
            ActivityMeasurementUnitParser.Parse(request.MeasurementUnit),
            cancellationToken);
}

/// <summary>Updates a measurement-based product requirement section.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateActivityRequirementSectionCommand : IRequest
{
    public Guid ActivityId { get; set; }
    public Guid SectionId { get; set; }
    public string? Name { get; init; }
    public decimal BasisQuantity { get; init; }
    public string MeasurementUnit { get; init; } = string.Empty;
}

public sealed class UpdateActivityRequirementSectionHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<UpdateActivityRequirementSectionCommand>
{
    public Task Handle(
        UpdateActivityRequirementSectionCommand request,
        CancellationToken cancellationToken) =>
        requirementService.UpdateSectionAsync(
            request.ActivityId,
            request.SectionId,
            request.Name,
            request.BasisQuantity,
            ActivityMeasurementUnitParser.Parse(request.MeasurementUnit),
            cancellationToken);
}

/// <summary>Moves a requirement section within an activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record MoveActivityRequirementSectionCommand : IRequest
{
    public Guid ActivityId { get; set; }
    public Guid SectionId { get; set; }
    public int TargetIndex { get; init; }
}

public sealed class MoveActivityRequirementSectionHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<MoveActivityRequirementSectionCommand>
{
    public Task Handle(
        MoveActivityRequirementSectionCommand request,
        CancellationToken cancellationToken) =>
        requirementService.MoveSectionAsync(
            request.ActivityId,
            request.SectionId,
            request.TargetIndex,
            cancellationToken);
}

/// <summary>Deletes a requirement section and its product relationships.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteActivityRequirementSectionCommand(
    Guid ActivityId,
    Guid SectionId) : IRequest;

public sealed class DeleteActivityRequirementSectionHandler(
    IActivityRequirementService requirementService)
    : IRequestHandler<DeleteActivityRequirementSectionCommand>
{
    public Task Handle(
        DeleteActivityRequirementSectionCommand request,
        CancellationToken cancellationToken) =>
        requirementService.DeleteSectionAsync(
            request.ActivityId,
            request.SectionId,
            cancellationToken);
}

public sealed class CreateActivityRequirementSectionValidator
    : AbstractValidator<CreateActivityRequirementSectionCommand>
{
    public CreateActivityRequirementSectionValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.Name)
            .MaximumLength(ActivityRequirementSection.MaxNameLength);
        RuleFor(request => request.BasisQuantity)
            .GreaterThan(0m)
            .PrecisionScale(18, 4, false);
        RuleFor(request => request.MeasurementUnit)
            .NotEmpty()
            .Must(value => ActivityMeasurementUnitCodes.TryParse(value, out _))
            .WithMessage("Unsupported activity measurement unit.");
    }
}

public sealed class UpdateActivityRequirementSectionValidator
    : AbstractValidator<UpdateActivityRequirementSectionCommand>
{
    public UpdateActivityRequirementSectionValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.Name)
            .MaximumLength(ActivityRequirementSection.MaxNameLength);
        RuleFor(request => request.BasisQuantity)
            .GreaterThan(0m)
            .PrecisionScale(18, 4, false);
        RuleFor(request => request.MeasurementUnit)
            .NotEmpty()
            .Must(value => ActivityMeasurementUnitCodes.TryParse(value, out _))
            .WithMessage("Unsupported activity measurement unit.");
    }
}

public sealed class MoveActivityRequirementSectionValidator
    : AbstractValidator<MoveActivityRequirementSectionCommand>
{
    public MoveActivityRequirementSectionValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
        RuleFor(request => request.TargetIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class DeleteActivityRequirementSectionValidator
    : AbstractValidator<DeleteActivityRequirementSectionCommand>
{
    public DeleteActivityRequirementSectionValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
        RuleFor(request => request.SectionId).NotEmpty();
    }
}

internal static class ActivityMeasurementUnitParser
{
    public static ActivityMeasurementUnit Parse(string value) =>
        ActivityMeasurementUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new ArgumentException(
                $"Unsupported activity measurement unit '{value}'.",
                nameof(value));
}
