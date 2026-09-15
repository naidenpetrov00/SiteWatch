using System.Data;
using Application.ActivityCatalog;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ActivityCatalog;

public sealed class ActivityCatalogService(ApplicationDbContext dbContext)
    : IActivityCatalogService
{
    public Task<Guid> CreateFolderAsync(
        string name,
        Guid? parentFolderId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                await LoadFolderAsync(parentFolderId, cancellationToken);
                await EnsureUniqueNameAsync(parentFolderId, name, null, cancellationToken);
                var sortOrder = await SiblingCountAsync(parentFolderId, cancellationToken);
                var folder = ActivityFolder.Create(name, parentFolderId, sortOrder);
                dbContext.ActivityCatalogNodes.Add(folder);
                return folder.Id;
            },
            "An item with this name already exists in the selected folder.",
            cancellationToken);

    public Task RenameFolderAsync(
        Guid folderId,
        string name,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var folder = await LoadNodeAsync<ActivityFolder>(folderId, cancellationToken);
                await EnsureUniqueNameAsync(
                    folder.ParentFolderId,
                    name,
                    folder.Id,
                    cancellationToken);
                folder.Rename(name);
            },
            "An item with this name already exists in the selected folder.",
            cancellationToken);

    public Task MoveFolderAsync(
        Guid folderId,
        Guid? targetParentFolderId,
        int targetIndex,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var folder = await LoadNodeAsync<ActivityFolder>(folderId, cancellationToken);
                var targetParent = await LoadFolderAsync(targetParentFolderId, cancellationToken);
                await EnsureFolderMoveDoesNotCreateCycleAsync(
                    folder.Id,
                    targetParentFolderId,
                    cancellationToken);
                await EnsureUniqueNameAsync(
                    targetParentFolderId,
                    folder.Name,
                    folder.Id,
                    cancellationToken);
                await MoveNodeAsync(folder, targetParent, targetIndex, cancellationToken);
            },
            "The folder could not be moved because the catalog changed or the destination conflicts.",
            cancellationToken);

    public Task DeleteFolderAsync(Guid folderId, CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var folder = await LoadNodeAsync<ActivityFolder>(folderId, cancellationToken);
                var hasChildren = await dbContext.ActivityCatalogNodes
                    .AnyAsync(node => node.ParentFolderId == folder.Id, cancellationToken);
                if (hasChildren)
                {
                    throw new ActivityCatalogConflictException(
                        "Only empty activity folders can be deleted.");
                }

                var siblings = await LoadSiblingsAsync(
                    folder.ParentFolderId,
                    folder.Id,
                    cancellationToken);
                Renumber(siblings);
                dbContext.ActivityCatalogNodes.Remove(folder);
            },
            "The folder could not be deleted because it is no longer empty or is referenced.",
            cancellationToken);

    public Task<Guid> CreateActivityAsync(
        string name,
        string? description,
        Guid? parentFolderId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                await LoadFolderAsync(parentFolderId, cancellationToken);
                await EnsureUniqueNameAsync(parentFolderId, name, null, cancellationToken);
                var sortOrder = await SiblingCountAsync(parentFolderId, cancellationToken);
                var activity = Activity.Create(name, description, parentFolderId, sortOrder);
                dbContext.ActivityCatalogNodes.Add(activity);
                return activity.Id;
            },
            "An item with this name already exists in the selected folder.",
            cancellationToken);

    public Task UpdateActivityAsync(
        Guid activityId,
        string name,
        string? description,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadNodeAsync<Activity>(activityId, cancellationToken);
                await EnsureUniqueNameAsync(
                    activity.ParentFolderId,
                    name,
                    activity.Id,
                    cancellationToken);
                activity.UpdateDetails(name, description);
            },
            "An item with this name already exists in the selected folder.",
            cancellationToken);

    public Task MoveActivityAsync(
        Guid activityId,
        Guid? targetParentFolderId,
        int targetIndex,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadNodeAsync<Activity>(activityId, cancellationToken);
                var targetParent = await LoadFolderAsync(targetParentFolderId, cancellationToken);
                await EnsureUniqueNameAsync(
                    targetParentFolderId,
                    activity.Name,
                    activity.Id,
                    cancellationToken);
                await MoveNodeAsync(activity, targetParent, targetIndex, cancellationToken);
            },
            "The activity could not be moved because the catalog changed or the destination conflicts.",
            cancellationToken);

    public Task SetActivityArchivedAsync(
        Guid activityId,
        bool archived,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadNodeAsync<Activity>(activityId, cancellationToken);
                if (archived)
                {
                    activity.Archive();
                }
                else
                {
                    activity.Restore();
                }
            },
            "The activity status could not be changed.",
            cancellationToken);

    public Task DeleteActivityAsync(Guid activityId, CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var activity = await LoadNodeAsync<Activity>(activityId, cancellationToken);
                var siblings = await LoadSiblingsAsync(
                    activity.ParentFolderId,
                    activity.Id,
                    cancellationToken);
                Renumber(siblings);
                dbContext.ActivityCatalogNodes.Remove(activity);
            },
            "This activity is referenced by retained data. Archive it instead of deleting it.",
            cancellationToken);

    private async Task MoveNodeAsync(
        ActivityCatalogNode node,
        ActivityFolder? targetParent,
        int targetIndex,
        CancellationToken cancellationToken)
    {
        var targetParentFolderId = targetParent?.Id;
        if (node.ParentFolderId == targetParentFolderId)
        {
            var siblings = await LoadSiblingsAsync(
                node.ParentFolderId,
                node.Id,
                cancellationToken);
            EnsureTargetIndex(targetIndex, siblings.Count);
            siblings.Insert(targetIndex, node);
            node.MoveTo(targetParent, targetIndex);
            Renumber(siblings);
            return;
        }

        var oldSiblings = await LoadSiblingsAsync(
            node.ParentFolderId,
            node.Id,
            cancellationToken);
        var targetSiblings = await LoadSiblingsAsync(
            targetParentFolderId,
            node.Id,
            cancellationToken);
        EnsureTargetIndex(targetIndex, targetSiblings.Count);

        node.MoveTo(targetParent, targetIndex);
        targetSiblings.Insert(targetIndex, node);
        Renumber(oldSiblings);
        Renumber(targetSiblings);
    }

    private async Task EnsureFolderMoveDoesNotCreateCycleAsync(
        Guid folderId,
        Guid? targetParentFolderId,
        CancellationToken cancellationToken)
    {
        if (!targetParentFolderId.HasValue)
        {
            return;
        }

        var parents = await dbContext.ActivityCatalogNodes
            .OfType<ActivityFolder>()
            .AsNoTracking()
            .Select(folder => new { folder.Id, folder.ParentFolderId })
            .ToDictionaryAsync(folder => folder.Id, cancellationToken);
        var visited = new HashSet<Guid>();
        var currentId = targetParentFolderId;

        while (currentId.HasValue)
        {
            if (currentId.Value == folderId)
            {
                throw new ActivityCatalogConflictException(
                    "A folder cannot be moved into itself or one of its descendants.");
            }

            if (!visited.Add(currentId.Value)
                || !parents.TryGetValue(currentId.Value, out var current))
            {
                throw new ActivityCatalogConflictException(
                    "The destination folder hierarchy is invalid.");
            }

            currentId = current.ParentFolderId;
        }
    }

    private async Task EnsureUniqueNameAsync(
        Guid? parentFolderId,
        string name,
        Guid? excludedNodeId,
        CancellationToken cancellationToken)
    {
        var normalizedName = ActivityCatalogNode.NormalizeNameKey(name);
        var exists = await dbContext.ActivityCatalogNodes.AnyAsync(
            node => node.ParentFolderId == parentFolderId
                && node.NormalizedName == normalizedName
                && (!excludedNodeId.HasValue || node.Id != excludedNodeId.Value),
            cancellationToken);
        if (exists)
        {
            throw new ActivityCatalogConflictException(
                "An item with this name already exists in the selected folder.");
        }
    }

    private async Task<ActivityFolder?> LoadFolderAsync(
        Guid? folderId,
        CancellationToken cancellationToken)
    {
        if (!folderId.HasValue)
        {
            return null;
        }

        return await LoadNodeAsync<ActivityFolder>(folderId.Value, cancellationToken);
    }

    private async Task<TNode> LoadNodeAsync<TNode>(
        Guid nodeId,
        CancellationToken cancellationToken)
        where TNode : ActivityCatalogNode
    {
        var node = await dbContext.ActivityCatalogNodes
            .OfType<TNode>()
            .SingleOrDefaultAsync(node => node.Id == nodeId, cancellationToken);
        Guard.Against.NotFound(nodeId, node);
        return node;
    }

    private Task<int> SiblingCountAsync(
        Guid? parentFolderId,
        CancellationToken cancellationToken) =>
        dbContext.ActivityCatalogNodes.CountAsync(
            node => node.ParentFolderId == parentFolderId,
            cancellationToken);

    private Task<List<ActivityCatalogNode>> LoadSiblingsAsync(
        Guid? parentFolderId,
        Guid excludedNodeId,
        CancellationToken cancellationToken) =>
        dbContext.ActivityCatalogNodes
            .Where(node => node.ParentFolderId == parentFolderId && node.Id != excludedNodeId)
            .OrderBy(node => node.SortOrder)
            .ThenBy(node => node.Id)
            .ToListAsync(cancellationToken);

    private static void EnsureTargetIndex(int targetIndex, int destinationCount)
    {
        if (targetIndex < 0 || targetIndex > destinationCount)
        {
            throw new ActivityCatalogConflictException(
                "The requested position is no longer available. Refresh the catalog and try again.");
        }
    }

    private static void Renumber(IReadOnlyList<ActivityCatalogNode> nodes)
    {
        for (var index = 0; index < nodes.Count; index++)
        {
            nodes[index].SetSortOrder(index);
        }
    }

    private async Task ExecuteMutationAsync(
        Func<Task> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                await mutation();
                return true;
            },
            persistenceConflictMessage,
            cancellationToken);
    }

    private async Task<TResult> ExecuteMutationAsync<TResult>(
        Func<Task<TResult>> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var result = await mutation();
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new ActivityCatalogConflictException(
                persistenceConflictMessage,
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
