using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.SeedWork.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public sealed class BlobInitializer(
    ApplicationDbContext dbContext,
    BlobServiceClient blobServiceClient,
    IImagesService imagesService,
    IVideosService videosService,
    ILogger<BlobInitializer> logger)
{
    private const string SeedAssetsDirectoryName = "Data/SeedAssets";
    private const string SeededBy = "System";
    private static readonly string[] SeedSiteAddresses = ["Vitosha 17", "Dondukov 11", "Kestenova Gora 24"];
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureContainersAsync(cancellationToken);

        var seedAssetsPath = Path.Combine(AppContext.BaseDirectory, SeedAssetsDirectoryName);
        if (!Directory.Exists(seedAssetsPath))
        {
            logger.LogInformation("Blob seeding skipped: seed assets directory {SeedAssetsPath} was not found.", seedAssetsPath);
            return;
        }

        var sites = await dbContext.Sites
            .Where(site => SeedSiteAddresses.Contains(site.Address.Value))
            .ToListAsync(cancellationToken);
        if (sites.Count == 0)
        {
            logger.LogWarning("Blob seeding skipped: no seeded sites are available.");
            return;
        }

        var missingAddresses = SeedSiteAddresses.Except(sites.Select(site => site.Address.Value)).ToArray();
        if (missingAddresses.Length > 0)
        {
            logger.LogWarning("Blob seeding will not include missing seeded sites: {Addresses}.", missingAddresses);
        }

        await SeedImagesAsync(Path.Combine(seedAssetsPath, "Images"), sites, cancellationToken);
        await SeedVideosAsync(Path.Combine(seedAssetsPath, "Videos"), sites, cancellationToken);
        await SeedFilesAsync(Path.Combine(seedAssetsPath, "Files"), sites, cancellationToken);
    }

    private async Task EnsureContainersAsync(CancellationToken cancellationToken)
    {
        foreach (var containerName in new[] { "images", "videos", "files" })
        {
            await blobServiceClient
                .GetBlobContainerClient(containerName)
                .CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private async Task SeedImagesAsync(string directory, IReadOnlyCollection<Site> sites, CancellationToken cancellationToken)
    {
        foreach (var assetPath in GetAssetPaths(directory, "image"))
        {
            if (!TryGetContentType(assetPath, out var contentType) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Image seed asset {AssetPath} has an unsupported content type.", assetPath);
                continue;
            }

            foreach (var site in sites)
            foreach (var category in site.MediaPolicy.AllowedImageCategories)
            {
                try
                {
                    await SeedImageAsync(assetPath, site, category, contentType, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to seed image {AssetPath} for site {SiteAddress} and category {Category}.", assetPath, site.Address.Value, category);
                }
            }
        }
    }

    private async Task SeedImageAsync(string assetPath, Site site, ImageCategory category, string contentType, CancellationToken cancellationToken)
    {
        var assetKey = GetAssetKey(assetPath);
        var imageId = CreateDeterministicGuid("image", assetKey, site.Address.Value, category.ToString());
        var thumbnailId = CreateDeterministicGuid("image-thumbnail", assetKey, site.Address.Value, category.ToString());
        var recordExists = await dbContext.SiteImages.AnyAsync(
            image => image.SiteId == site.Id && image.ImageId == imageId, cancellationToken);

        var imageContainer = blobServiceClient.GetBlobContainerClient("images");
        var originalBlob = imageContainer.GetBlobClient(imageId.ToString());
        if (!(await originalBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var originalStream = File.OpenRead(assetPath);
            await originalBlob.UploadAsync(originalStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        }

        var thumbnailBlob = imageContainer.GetBlobClient(thumbnailId.ToString());
        if (!(await thumbnailBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var originalStream = File.OpenRead(assetPath);
            await using var thumbnailStream = await imagesService.CreateThumbnailAsync(originalStream, cancellationToken);
            await thumbnailBlob.UploadAsync(thumbnailStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        }

        if (!recordExists)
        {
            var image = new SiteImage(site.Id, imageId, thumbnailId, category);
            SetAuditValues(image);
            dbContext.SiteImages.Add(image);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedVideosAsync(string directory, IReadOnlyCollection<Site> sites, CancellationToken cancellationToken)
    {
        foreach (var assetPath in GetAssetPaths(directory, "video"))
        {
            if (!TryGetContentType(assetPath, out var contentType) || !contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Video seed asset {AssetPath} has an unsupported content type.", assetPath);
                continue;
            }

            foreach (var site in sites)
            foreach (var category in site.MediaPolicy.AllowedVideoCategories)
            {
                try
                {
                    await SeedVideoAsync(assetPath, site, category, contentType, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to seed video {AssetPath} for site {SiteAddress} and category {Category}.", assetPath, site.Address.Value, category);
                }
            }
        }
    }

    private async Task SeedVideoAsync(string assetPath, Site site, VideoCategory category, string contentType, CancellationToken cancellationToken)
    {
        var assetKey = GetAssetKey(assetPath);
        var videoId = CreateDeterministicGuid("video", assetKey, site.Address.Value, category.ToString());
        var snapshotId = CreateDeterministicGuid("video-snapshot", assetKey, site.Address.Value, category.ToString());
        var existingVideo = await dbContext.SiteVideos.SingleOrDefaultAsync(
            video => video.SiteId == site.Id && video.VideoId == videoId, cancellationToken);

        var videoBlob = blobServiceClient.GetBlobContainerClient("videos").GetBlobClient(videoId.ToString());
        if (!(await videoBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var videoStream = File.OpenRead(assetPath);
            await videoBlob.UploadAsync(videoStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        }

        var snapshotBlob = blobServiceClient.GetBlobContainerClient("images").GetBlobClient(snapshotId.ToString());
        if (!(await snapshotBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var videoStream = File.OpenRead(assetPath);
            await using var snapshotStream = await videosService.CreateSnapshotAsync(videoStream, cancellationToken);
            await snapshotBlob.UploadAsync(snapshotStream, new BlobHttpHeaders { ContentType = "image/jpeg" }, cancellationToken: cancellationToken);
        }

        if (existingVideo is null)
        {
            var durationSeconds = await GetVideoDurationSecondsAsync(assetPath, cancellationToken);
            var video = new SiteVideo(site.Id, videoId, snapshotId, durationSeconds, category);
            SetAuditValues(video);
            dbContext.SiteVideos.Add(video);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedFilesAsync(string directory, IReadOnlyCollection<Site> sites, CancellationToken cancellationToken)
    {
        foreach (var assetPath in GetAssetPaths(directory, "file"))
        {
            var contentType = TryGetContentType(assetPath, out var detectedContentType)
                ? detectedContentType
                : "application/octet-stream";

            foreach (var site in sites)
            foreach (var documentType in Enum.GetValues<FileDocumentType>())
            {
                try
                {
                    await SeedFileAsync(assetPath, site, documentType, contentType, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to seed file {AssetPath} for site {SiteAddress} and document type {DocumentType}.", assetPath, site.Address.Value, documentType);
                }
            }
        }
    }

    private async Task SeedFileAsync(string assetPath, Site site, FileDocumentType documentType, string contentType, CancellationToken cancellationToken)
    {
        var fileId = CreateDeterministicGuid("file", GetAssetKey(assetPath), site.Address.Value, documentType.ToString());
        var recordExists = await dbContext.SiteFiles.AnyAsync(
            file => file.SiteId == site.Id && file.FileId == fileId, cancellationToken);
        var blob = blobServiceClient.GetBlobContainerClient("files").GetBlobClient(fileId.ToString());

        if (!(await blob.ExistsAsync(cancellationToken)).Value)
        {
            await using var fileStream = File.OpenRead(assetPath);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        }

        if (!recordExists)
        {
            var file = new SiteFile(site.Id, fileId, Path.GetFileName(assetPath), contentType, documentType);
            SetAuditValues(file);
            dbContext.SiteFiles.Add(file);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private IEnumerable<string> GetAssetPaths(string directory, string assetType)
    {
        if (!Directory.Exists(directory))
        {
            logger.LogInformation("{AssetType} seed folder {Directory} is empty or missing.", assetType, directory);
            return [];
        }

        try
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(path => !string.Equals(Path.GetFileName(path), ".gitkeep", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not enumerate {AssetType} seed folder {Directory}.", assetType, directory);
            return [];
        }
    }

    private static bool TryGetContentType(string assetPath, out string contentType) =>
        ContentTypeProvider.TryGetContentType(assetPath, out contentType!);

    private static string GetAssetKey(string assetPath) =>
        Path.GetRelativePath(Path.Combine(AppContext.BaseDirectory, SeedAssetsDirectoryName), assetPath)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static Guid CreateDeterministicGuid(params string[] values)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', values)));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static void SetAuditValues(Domain.SeedWork.BaseAuditableEntity entity)
    {
        var now = DateTimeOffset.UtcNow;
        entity.Created = now;
        entity.CreatedBy = SeededBy;
        entity.LastModified = now;
        entity.LastModifiedBy = SeededBy;
    }

    private static async Task<int?> GetVideoDurationSecondsAsync(string assetPath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("error");
        startInfo.ArgumentList.Add("-show_entries");
        startInfo.ArgumentList.Add("format=duration");
        startInfo.ArgumentList.Add("-of");
        startInfo.ArgumentList.Add("default=noprint_wrappers=1:nokey=1");
        startInfo.ArgumentList.Add(assetPath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start ffprobe.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        _ = await errorTask;

        if (process.ExitCode != 0 || !double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var duration))
        {
            return null;
        }

        return Math.Max(0, (int)Math.Round(duration, MidpointRounding.AwayFromZero));
    }
}
