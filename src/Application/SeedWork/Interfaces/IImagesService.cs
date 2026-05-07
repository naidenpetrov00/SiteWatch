using Application.Sites.Images.Queries;

namespace Application.SeedWork.Interfaces;

public interface IImagesService
{
    Task<Stream> CreateThumbnailAsync(
        Stream originalStream,
        CancellationToken cancellationToken = default);

    Task<List<SiteImageIdsDto>> GetImagesIdsBySiteId(Guid siteId);

    Task AddImageIdsToSiteAsync(Guid requestSiteId, Guid resultOriginalFileId, Guid resultThumbnailFileId,
        CancellationToken cancellationToken);
}