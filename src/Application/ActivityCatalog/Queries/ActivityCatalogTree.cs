using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ActivityCatalog.Queries;

/// <summary>Loads every node in the administrator activity catalog.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ActivityCatalogTreeQuery : IRequest<List<ActivityCatalogNodeDto>>;

/// <summary>Represents one folder or activity in the complete catalog tree.</summary>
public sealed record ActivityCatalogNodeDto(
    Guid Id,
    string Kind,
    Guid? ParentFolderId,
    string Name,
    int SortOrder,
    int? NumberId,
    string? Status);

public sealed class ActivityCatalogTreeHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ActivityCatalogTreeQuery, List<ActivityCatalogNodeDto>>
{
    public async Task<List<ActivityCatalogNodeDto>> Handle(
        ActivityCatalogTreeQuery request,
        CancellationToken cancellationToken)
    {
        var nodes = await dbContext.ActivityCatalogNodes
            .AsNoTracking()
            .OrderBy(node => node.ParentFolderId)
            .ThenBy(node => node.SortOrder)
            .ThenBy(node => node.Name)
            .ToListAsync(cancellationToken);

        return nodes.Select(node => node switch
            {
                Activity activity => new ActivityCatalogNodeDto(
                    activity.Id,
                    "activity",
                    activity.ParentFolderId,
                    activity.Name,
                    activity.SortOrder,
                    activity.NumberId,
                    activity.Status.ToString()),
                _ => new ActivityCatalogNodeDto(
                    node.Id,
                    "folder",
                    node.ParentFolderId,
                    node.Name,
                    node.SortOrder,
                    null,
                    null)
            })
            .ToList();
    }
}
