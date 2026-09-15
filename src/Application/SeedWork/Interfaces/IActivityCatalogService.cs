namespace Application.SeedWork.Interfaces;

public interface IActivityCatalogService
{
    Task<Guid> CreateFolderAsync(
        string name,
        Guid? parentFolderId,
        CancellationToken cancellationToken);

    Task RenameFolderAsync(Guid folderId, string name, CancellationToken cancellationToken);

    Task MoveFolderAsync(
        Guid folderId,
        Guid? targetParentFolderId,
        int targetIndex,
        CancellationToken cancellationToken);

    Task DeleteFolderAsync(Guid folderId, CancellationToken cancellationToken);

    Task<Guid> CreateActivityAsync(
        string name,
        string? description,
        Guid? parentFolderId,
        CancellationToken cancellationToken);

    Task UpdateActivityAsync(
        Guid activityId,
        string name,
        string? description,
        CancellationToken cancellationToken);

    Task MoveActivityAsync(
        Guid activityId,
        Guid? targetParentFolderId,
        int targetIndex,
        CancellationToken cancellationToken);

    Task SetActivityArchivedAsync(
        Guid activityId,
        bool archived,
        CancellationToken cancellationToken);

    Task DeleteActivityAsync(Guid activityId, CancellationToken cancellationToken);
}
