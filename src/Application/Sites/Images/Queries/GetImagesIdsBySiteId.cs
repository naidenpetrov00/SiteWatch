using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Sites.Images.Queries;

public sealed record GetImagesIdsBySiteIdQuery : IRequest<List<SiteImageIdsDto>>
{
    public Guid SiteId { get; init; }
}

public sealed class GetImagesIdsBySiteIdHandler(IImagesService imagesService)
    : IRequestHandler<GetImagesIdsBySiteIdQuery, List<SiteImageIdsDto>>
{
    public Task<List<SiteImageIdsDto>> Handle(GetImagesIdsBySiteIdQuery request, CancellationToken cancellationToken)
        =>
            imagesService.GetImagesIdsBySiteId(request.SiteId);
}