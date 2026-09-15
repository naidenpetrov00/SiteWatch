using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ActivityCatalog.Queries;

/// <summary>Loads the complete editable details of one activity.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ActivityDetailsQuery(Guid ActivityId) : IRequest<ActivityDetailsDto>;

/// <summary>Represents the editable details of an activity.</summary>
public sealed record ActivityDetailsDto(
    Guid Id,
    int NumberId,
    string Name,
    string? Description,
    string Status,
    Guid? ParentFolderId,
    int SortOrder);

public sealed class ActivityDetailsQueryValidator : AbstractValidator<ActivityDetailsQuery>
{
    public ActivityDetailsQueryValidator()
    {
        RuleFor(request => request.ActivityId).NotEmpty();
    }
}

public sealed class ActivityDetailsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ActivityDetailsQuery, ActivityDetailsDto>
{
    public async Task<ActivityDetailsDto> Handle(
        ActivityDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var activity = await dbContext.ActivityCatalogNodes
            .OfType<Activity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                activity => activity.Id == request.ActivityId,
                cancellationToken);

        Guard.Against.NotFound(request.ActivityId, activity);

        return new ActivityDetailsDto(
            activity.Id,
            activity.NumberId,
            activity.Name,
            activity.Description,
            activity.Status.ToString(),
            activity.ParentFolderId,
            activity.SortOrder);
    }
}
