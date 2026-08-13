using Application.Sites.Images.Queries;

namespace Application.SeedWork.Interfaces;

public interface IImagesService
{
    Task<Stream> CreateThumbnailAsync(
        Stream originalStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<List<SiteImageIdsDto>> GetImagesIdsBySiteId(
        Guid siteId,
        CancellationToken cancellationToken = default);

    Task AddImageIdsToSiteAsync(Guid requestSiteId, Guid resultOriginalFileId, Guid resultThumbnailFileId,
        string category,
        CancellationToken cancellationToken);

    Task DeleteImageIdFromSiteAsync(Guid imageId, CancellationToken cancellationToken = default);
}
