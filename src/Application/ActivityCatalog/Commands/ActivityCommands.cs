using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.ActivityCatalog.Commands;

/// <summary>Creates a root or nested activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateActivityCommand(
    string Name,
    string? Description,
    Guid? ParentFolderId) : IRequest<Guid>;

public sealed class CreateActivityHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<CreateActivityCommand, Guid>
{
    public Task<Guid> Handle(
        CreateActivityCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.CreateActivityAsync(
            request.Name,
            request.Description,
            request.ParentFolderId,
            cancellationToken);
}

/// <summary>Updates the editable details of an activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateActivityCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class UpdateActivityHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<UpdateActivityCommand>
{
    public Task Handle(
        UpdateActivityCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.UpdateActivityAsync(
            request.Id,
            request.Name,
            request.Description,
            cancellationToken);
}

/// <summary>Moves an activity and sets its position among the destination siblings.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record MoveActivityCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid? TargetParentFolderId { get; init; }
    public int TargetIndex { get; init; }
}

public sealed class MoveActivityHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<MoveActivityCommand>
{
    public Task Handle(
        MoveActivityCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.MoveActivityAsync(
            request.Id,
            request.TargetParentFolderId,
            request.TargetIndex,
            cancellationToken);
}

/// <summary>Archives or restores an activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record SetActivityArchivedCommand(Guid Id, bool Archived) : IRequest;

public sealed class SetActivityArchivedHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<SetActivityArchivedCommand>
{
    public Task Handle(
        SetActivityArchivedCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.SetActivityArchivedAsync(
            request.Id,
            request.Archived,
            cancellationToken);
}

/// <summary>Deletes an activity when it is not referenced by retained data.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteActivityCommand(Guid Id) : IRequest;

public sealed class DeleteActivityHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<DeleteActivityCommand>
{
    public Task Handle(
        DeleteActivityCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.DeleteActivityAsync(request.Id, cancellationToken);
}

public sealed class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(ActivityCatalogNode.MaxNameLength);
        RuleFor(request => request.Description)
            .MaximumLength(Activity.MaxDescriptionLength);
        RuleFor(request => request.ParentFolderId)
            .NotEqual(Guid.Empty)
            .When(request => request.ParentFolderId.HasValue);
    }
}

public sealed class UpdateActivityValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(ActivityCatalogNode.MaxNameLength);
        RuleFor(request => request.Description)
            .MaximumLength(Activity.MaxDescriptionLength);
    }
}

public sealed class MoveActivityValidator : AbstractValidator<MoveActivityCommand>
{
    public MoveActivityValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.TargetParentFolderId)
            .NotEqual(Guid.Empty)
            .When(request => request.TargetParentFolderId.HasValue);
        RuleFor(request => request.TargetIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class SetActivityArchivedValidator
    : AbstractValidator<SetActivityArchivedCommand>
{
    public SetActivityArchivedValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}

public sealed class DeleteActivityValidator : AbstractValidator<DeleteActivityCommand>
{
    public DeleteActivityValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
