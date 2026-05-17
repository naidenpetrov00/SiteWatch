using Application.Sites.Videos.Queries;
using Domain.SeedWork.Enums;

namespace Application.SeedWork.Interfaces;

public interface IVideosService
{
    Task<Stream> CreateSnapshotAsync(
        Stream originalStream,
        CancellationToken cancellationToken = default);

    Task<List<SiteVideoIdsDto>> GetVideosIdsBySiteId(Guid siteId);

    Task AddVideoIdsToSiteAsync(
        Guid requestSiteId,
        Guid resultVideoFileId,
        Guid resultSnapshotFileId,
        VideoCategory category,
        CancellationToken cancellationToken);
}
