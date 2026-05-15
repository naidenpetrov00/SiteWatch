using Application.SeedWork.Interfaces;
using Application.Sites.Images.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Sites.Services;

public class ImagesService(IApplicationDbContext dbContext) : IImagesService
{
    public async Task<Stream> CreateThumbnailAsync(Stream originalStream, CancellationToken cancellationToken = default)
    {
        originalStream.Position = 0;

        using var image = await Image.LoadAsync(originalStream, cancellationToken);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(400, 400),
            Mode = ResizeMode.Max,
        }));

        var output = new MemoryStream();

        await image.SaveAsJpegAsync(output, new JpegEncoder
        {
            Quality = 75
        }, cancellationToken);

        output.Position = 0;

        return output;
    }


    public Task<List<SiteImageIdsDto>> GetImagesIdsBySiteId(Guid siteId) => dbContext.SiteImages
        .AsNoTracking()
        .Where(siteImage => siteImage.SiteId == siteId)
        .Select(siteImage => new SiteImageIdsDto(siteImage.ImageId, siteImage.ThumbnailImageId)).ToListAsync();

    public async Task AddImageIdsToSiteAsync(Guid requestSiteId, Guid resultOriginalFileId, Guid resultThumbnailFileId,
        ImageCategory category,
        CancellationToken cancellationToken = default)
    {
        dbContext.SiteImages.Add(new SiteImage(requestSiteId, resultOriginalFileId, resultThumbnailFileId,category));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}