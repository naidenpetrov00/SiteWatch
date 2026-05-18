using Application.Sites.Images.Queries;
using Domain.SeedWork.Enums;

namespace Application.SeedWork.Interfaces;

public interface IImagesService
{
    Task<Stream> CreateThumbnailAsync(
        Stream originalStream,
        CancellationToken cancellationToken = default);

    Task<List<SiteImageIdsDto>> GetImagesIdsBySiteId(Guid siteId);

    Task AddImageIdsToSiteAsync(Guid requestSiteId, Guid resultOriginalFileId, Guid resultThumbnailFileId,
        ImageCategory category,
        CancellationToken cancellationToken);

    Task DeleteImageIdFromSiteAsync(Guid imageId, CancellationToken cancellationToken = default);
}
