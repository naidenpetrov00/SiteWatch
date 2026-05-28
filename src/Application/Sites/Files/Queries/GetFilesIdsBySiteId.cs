using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Sites.Files.Queries;

public sealed record GetFilesIdsBySiteIdQuery : IRequest<List<SiteFileIdsDto>>
{
    public Guid SiteId { get; init; }
}

public sealed class GetFilesIdsBySiteIdHandler(IFilesService filesService)
    : IRequestHandler<GetFilesIdsBySiteIdQuery, List<SiteFileIdsDto>>
{
    public Task<List<SiteFileIdsDto>> Handle(
        GetFilesIdsBySiteIdQuery request,
        CancellationToken cancellationToken
    ) => filesService.GetFilesIdsBySiteId(request.SiteId);
}
