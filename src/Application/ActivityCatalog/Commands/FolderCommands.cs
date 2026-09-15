using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.ActivityCatalog.Commands;

/// <summary>Creates a root or nested activity folder.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateActivityFolderCommand(
    string Name,
    Guid? ParentFolderId) : IRequest<Guid>;

public sealed class CreateActivityFolderHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<CreateActivityFolderCommand, Guid>
{
    public Task<Guid> Handle(
        CreateActivityFolderCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.CreateFolderAsync(
            request.Name,
            request.ParentFolderId,
            cancellationToken);
}

/// <summary>Renames an activity folder.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RenameActivityFolderCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
}

public sealed class RenameActivityFolderHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<RenameActivityFolderCommand>
{
    public Task Handle(
        RenameActivityFolderCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.RenameFolderAsync(request.Id, request.Name, cancellationToken);
}

/// <summary>Moves an activity folder and sets its position among the destination siblings.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record MoveActivityFolderCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid? TargetParentFolderId { get; init; }
    public int TargetIndex { get; init; }
}

public sealed class MoveActivityFolderHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<MoveActivityFolderCommand>
{
    public Task Handle(
        MoveActivityFolderCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.MoveFolderAsync(
            request.Id,
            request.TargetParentFolderId,
            request.TargetIndex,
            cancellationToken);
}

/// <summary>Deletes an empty activity folder.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteActivityFolderCommand(Guid Id) : IRequest;

public sealed class DeleteActivityFolderHandler(IActivityCatalogService activityCatalogService)
    : IRequestHandler<DeleteActivityFolderCommand>
{
    public Task Handle(
        DeleteActivityFolderCommand request,
        CancellationToken cancellationToken) =>
        activityCatalogService.DeleteFolderAsync(request.Id, cancellationToken);
}

public sealed class CreateActivityFolderValidator
    : AbstractValidator<CreateActivityFolderCommand>
{
    public CreateActivityFolderValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(ActivityCatalogNode.MaxNameLength);
        RuleFor(request => request.ParentFolderId)
            .NotEqual(Guid.Empty)
            .When(request => request.ParentFolderId.HasValue);
    }
}

public sealed class RenameActivityFolderValidator
    : AbstractValidator<RenameActivityFolderCommand>
{
    public RenameActivityFolderValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(ActivityCatalogNode.MaxNameLength);
    }
}

public sealed class MoveActivityFolderValidator
    : AbstractValidator<MoveActivityFolderCommand>
{
    public MoveActivityFolderValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.TargetParentFolderId)
            .NotEqual(Guid.Empty)
            .When(request => request.TargetParentFolderId.HasValue);
        RuleFor(request => request.TargetIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class DeleteActivityFolderValidator
    : AbstractValidator<DeleteActivityFolderCommand>
{
    public DeleteActivityFolderValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
