using Application.Sites.Files.Queries;
using Domain.SeedWork.Enums;

namespace Application.SeedWork.Interfaces;

public interface IFilesService
{
    Task<List<SiteFileIdsDto>> GetFilesIdsBySiteId(Guid siteId);

    Task AddFileIdsToSiteAsync(
        Guid requestSiteId,
        Guid resultFileId,
        string fileName,
        string contentType,
        FileDocumentType documentType,
        CancellationToken cancellationToken = default);

    Task DeleteFileIdFromSiteAsync(Guid fileId, CancellationToken cancellationToken = default);
}
