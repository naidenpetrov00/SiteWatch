using Application.SeedWork.Interfaces;
using Application.Sites.Images.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using FluentValidation.Results;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Sites.Services;

public class ImagesService(IApplicationDbContext dbContext) : IImagesService
{
    public async Task<Stream> CreateThumbnailAsync(
        Stream originalStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        originalStream.Position = 0;

        try
        {
            if (IsHeifContentType(contentType))
            {
                return CreateHeifThumbnail(originalStream, contentType);
            }

            using var image = await Image.LoadAsync(originalStream, cancellationToken);
            var detectedContentType = image.Metadata.DecodedImageFormat?.DefaultMimeType;
            if (!string.Equals(contentType, detectedContentType, StringComparison.OrdinalIgnoreCase))
            {
                throw InvalidImage();
            }

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Max,
            }));

            var output = new MemoryStream();
            try
            {
                await image.SaveAsJpegAsync(output, new JpegEncoder
                {
                    Quality = 75
                }, cancellationToken);

                output.Position = 0;
                return output;
            }
            catch
            {
                await output.DisposeAsync();
                throw;
            }
        }
        catch (Exception exception) when (exception is UnknownImageFormatException
                                          or InvalidImageContentException
                                          or MagickException
                                          or NotSupportedException)
        {
            throw InvalidImage();
        }
    }

    private static Stream CreateHeifThumbnail(Stream originalStream, string contentType)
    {
        using var image = new MagickImage(originalStream);
        if (image.Format != MagickFormat.Heic || !IsHeifContentType(contentType))
        {
            throw InvalidImage();
        }

        image.AutoOrient();
        image.Resize(400, 400);
        image.Format = MagickFormat.Jpeg;
        image.Quality = 75;

        var output = new MemoryStream();
        try
        {
            image.Write(output);
            output.Position = 0;
            return output;
        }
        catch
        {
            output.Dispose();
            throw;
        }
    }

    private static bool IsHeifContentType(string contentType) =>
        contentType is "image/heic" or "image/heif";

    private static ValidationException InvalidImage() =>
        new([new ValidationFailure(
            "file",
            "The image content is invalid or does not match its declared type.")]);


    public Task<List<SiteImageIdsDto>> GetImagesIdsBySiteId(Guid siteId) => dbContext.SiteImages
        .AsNoTracking()
        .Where(siteImage => siteImage.SiteId == siteId)
        .OrderByDescending(siteImage => siteImage.Created)
        .Select(siteImage => new SiteImageIdsDto(
            siteImage.ImageId,
            siteImage.ThumbnailImageId,
            siteImage.Category.ToString(),
            siteImage.Created))
        .ToListAsync();

    public async Task AddImageIdsToSiteAsync(Guid requestSiteId, Guid resultOriginalFileId, Guid resultThumbnailFileId,
        ImageCategory category,
        CancellationToken cancellationToken = default)
    {
        dbContext.SiteImages.Add(new SiteImage(requestSiteId, resultOriginalFileId, resultThumbnailFileId,category));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteImageIdFromSiteAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var siteImage = await dbContext.SiteImages.FirstOrDefaultAsync(si => si.ImageId == imageId, cancellationToken);

        if (siteImage is null)
        {
            return;
        }

        var site = await dbContext.Sites.FirstAsync(s => s.Id == siteImage.SiteId, cancellationToken);
        site.RemoveImage(siteImage);
        dbContext.SiteImages.Remove(siteImage);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
