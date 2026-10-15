using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.SeedWork.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class Atelier48VMediaSeedData(
    ApplicationDbContext dbContext,
    BlobServiceClient blobServiceClient,
    IImagesService imagesService,
    IVideosService videosService,
    ILogger<Atelier48VMediaSeedData> logger)
{
    private const string SeedAssetsRelativePath = "Data/SeedAssets/Sites/Ателие 48В";
    private const string DrawingsDirectoryName = "Ателие 48В Чертежи строител";
    private const string SeededBy = "System";

    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
    private static readonly IReadOnlyDictionary<string, string> RoomByNormalizedPrefix =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [NormalizeRoomKey("Антре")] = "Антре",
            [NormalizeRoomKey("Баня")] = "Баня",
            [NormalizeRoomKey("Кухня")] = "Кухня",
            [NormalizeRoomKey("Спалня")] = "Спалня",
            [NormalizeRoomKey("Спланя")] = "Спалня",
            [NormalizeRoomKey("Хол")] = "Хол",
        };
    private static readonly HashSet<string> SupportedVideoContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/quicktime",
            "video/webm",
        };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var site = await dbContext.Sites.SingleOrDefaultAsync(
            candidate =>
                candidate.Name.Value == Atelier48VSiteSeedData.SiteName
                && candidate.Address.Value == Atelier48VSiteSeedData.SiteAddress,
            cancellationToken);
        if (site is null)
        {
            logger.LogWarning(
                "Media for site {SiteName} was not seeded because the site was not found.",
                Atelier48VSiteSeedData.SiteName);
            return;
        }

        var seedAssetsPath = Path.Combine(
            AppContext.BaseDirectory,
            SeedAssetsRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var assetPaths = GetAssetPaths(seedAssetsPath);
        if (assetPaths.Count == 0)
        {
            return;
        }

        var photoCount = 0;
        var videoCount = 0;
        var drawingCount = 0;
        var interiorDesignCount = 0;
        var skippedCount = 0;

        foreach (var assetPath in assetPaths)
        {
            var assetKey = GetAssetKey(seedAssetsPath, assetPath);
            if (!CanReadSource(assetPath))
            {
                skippedCount++;
                continue;
            }

            if (!ContentTypeProvider.TryGetContentType(assetPath, out var contentType))
            {
                logger.LogWarning(
                    "Atelier 48V seed asset {AssetPath} has an unsupported file extension and was skipped.",
                    assetPath);
                skippedCount++;
                continue;
            }

            try
            {
                if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    var room = ResolveRoom(assetPath);
                    await SeedImageAsync(
                        assetPath,
                        assetKey,
                        contentType,
                        room,
                        site.Id,
                        cancellationToken);
                    photoCount++;
                    continue;
                }

                if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                {
                    if (!SupportedVideoContentTypes.Contains(contentType))
                    {
                        logger.LogWarning(
                            "Atelier 48V video {AssetPath} has unsupported content type {ContentType} and was skipped.",
                            assetPath,
                            contentType);
                        skippedCount++;
                        continue;
                    }

                    var room = ResolveRoom(assetPath);
                    await SeedVideoAsync(
                        assetPath,
                        assetKey,
                        contentType,
                        room,
                        site.Id,
                        cancellationToken);
                    videoCount++;
                    continue;
                }

                if (string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                    && !HasPdfSignature(assetPath))
                {
                    logger.LogWarning(
                        "Atelier 48V document {AssetPath} does not contain a PDF signature and was skipped.",
                        assetPath);
                    skippedCount++;
                    continue;
                }

                var documentType = IsConstructionDrawing(assetKey)
                    ? FileDocumentType.DrawingOrSchema
                    : FileDocumentType.InteriorDesign;
                await SeedFileAsync(
                    assetPath,
                    assetKey,
                    contentType,
                    documentType,
                    site.Id,
                    cancellationToken);
                if (documentType == FileDocumentType.DrawingOrSchema)
                {
                    drawingCount++;
                }
                else
                {
                    interiorDesignCount++;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                dbContext.ChangeTracker.Clear();
                logger.LogWarning(
                    exception,
                    "Atelier 48V seed asset {AssetPath} could not be imported and was skipped.",
                    assetPath);
                skippedCount++;
            }
        }

        if (videoCount == 0)
        {
            logger.LogInformation(
                "No supported video source files were supplied for site {SiteName}.",
                Atelier48VSiteSeedData.SiteName);
        }

        logger.LogInformation(
            "Ensured Atelier 48V media: {PhotoCount} photos, {VideoCount} videos, {DrawingCount} drawings and {InteriorDesignCount} interior-design files; skipped {SkippedCount} files.",
            photoCount,
            videoCount,
            drawingCount,
            interiorDesignCount,
            skippedCount);
    }

    private async Task SeedImageAsync(
        string assetPath,
        string assetKey,
        string contentType,
        string room,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var imageId = CreateDeterministicGuid("atelier-48v", "image", assetKey);
        var thumbnailId = CreateDeterministicGuid("atelier-48v", "image-thumbnail", assetKey);
        var imageContainer = blobServiceClient.GetBlobContainerClient("images");
        var originalBlob = imageContainer.GetBlobClient(imageId.ToString());
        if (!(await originalBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var source = File.OpenRead(assetPath);
            await originalBlob.UploadAsync(
                source,
                CreateUploadOptions(contentType, assetPath, assetKey, inline: true),
                cancellationToken);
        }

        var thumbnailBlob = imageContainer.GetBlobClient(thumbnailId.ToString());
        if (!(await thumbnailBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var source = File.OpenRead(assetPath);
            await using var thumbnail = await imagesService.CreateThumbnailAsync(
                source,
                contentType,
                cancellationToken);
            await thumbnailBlob.UploadAsync(
                thumbnail,
                CreateUploadOptions("image/jpeg", assetPath, assetKey, inline: true),
                cancellationToken);
        }

        var image = await dbContext.SiteImages.SingleOrDefaultAsync(
            candidate => candidate.SiteId == siteId && candidate.ImageId == imageId,
            cancellationToken);
        if (image is null)
        {
            image = new SiteImage(siteId, imageId, thumbnailId, room);
            SetAuditValues(image);
            dbContext.SiteImages.Add(image);
        }
        else if (!string.Equals(image.Category, room, StringComparison.Ordinal))
        {
            image.ChangeCategory(room);
            SetAuditValues(image, preserveCreated: true);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedVideoAsync(
        string assetPath,
        string assetKey,
        string contentType,
        string room,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var videoId = CreateDeterministicGuid("atelier-48v", "video", assetKey);
        var snapshotId = CreateDeterministicGuid("atelier-48v", "video-snapshot", assetKey);
        var videoContainer = blobServiceClient.GetBlobContainerClient("videos");
        var videoBlob = videoContainer.GetBlobClient(videoId.ToString());
        if (!(await videoBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var source = File.OpenRead(assetPath);
            await videoBlob.UploadAsync(
                source,
                CreateUploadOptions(contentType, assetPath, assetKey, inline: true),
                cancellationToken);
        }

        var snapshotBlob = blobServiceClient
            .GetBlobContainerClient("images")
            .GetBlobClient(snapshotId.ToString());
        if (!(await snapshotBlob.ExistsAsync(cancellationToken)).Value)
        {
            await using var source = File.OpenRead(assetPath);
            await using var snapshot = await videosService.CreateSnapshotAsync(
                source,
                cancellationToken);
            await snapshotBlob.UploadAsync(
                snapshot,
                CreateUploadOptions("image/jpeg", assetPath, assetKey, inline: true),
                cancellationToken);
        }

        var video = await dbContext.SiteVideos.SingleOrDefaultAsync(
            candidate => candidate.SiteId == siteId && candidate.VideoId == videoId,
            cancellationToken);
        if (video is null)
        {
            var durationSeconds = await GetVideoDurationSecondsAsync(assetPath, cancellationToken);
            video = new SiteVideo(siteId, videoId, snapshotId, durationSeconds, room);
            SetAuditValues(video);
            dbContext.SiteVideos.Add(video);
        }
        else if (!string.Equals(video.Category, room, StringComparison.Ordinal))
        {
            video.ChangeCategory(room);
            SetAuditValues(video, preserveCreated: true);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedFileAsync(
        string assetPath,
        string assetKey,
        string contentType,
        FileDocumentType documentType,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var fileId = CreateDeterministicGuid(
            "atelier-48v",
            "file",
            assetKey,
            documentType.ToString());
        var blob = blobServiceClient.GetBlobContainerClient("files").GetBlobClient(fileId.ToString());
        if (!(await blob.ExistsAsync(cancellationToken)).Value)
        {
            await using var source = File.OpenRead(assetPath);
            await blob.UploadAsync(
                source,
                CreateUploadOptions(contentType, assetPath, assetKey, inline: false),
                cancellationToken);
        }

        var recordExists = await dbContext.SiteFiles.AnyAsync(
            candidate => candidate.SiteId == siteId && candidate.FileId == fileId,
            cancellationToken);
        if (!recordExists)
        {
            var file = new SiteFile(
                siteId,
                fileId,
                Path.GetFileName(assetPath),
                contentType,
                documentType);
            SetAuditValues(file);
            dbContext.SiteFiles.Add(file);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private string ResolveRoom(string assetPath)
    {
        var firstWord = Path.GetFileNameWithoutExtension(assetPath)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        var normalizedPrefix = NormalizeRoomKey(firstWord);
        if (RoomByNormalizedPrefix.TryGetValue(normalizedPrefix, out var room))
        {
            return room;
        }

        logger.LogWarning(
            "Atelier 48V media file {FileName} has no recognized room prefix and was assigned to {FallbackCategory}.",
            Path.GetFileName(assetPath),
            SiteMediaPolicy.OtherCategory);
        return SiteMediaPolicy.OtherCategory;
    }

    private IReadOnlyList<string> GetAssetPaths(string directory)
    {
        if (!Directory.Exists(directory))
        {
            logger.LogWarning(
                "Atelier 48V media seeding was skipped because source directory {Directory} is missing.",
                directory);
            return [];
        }

        try
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(path => !string.Equals(
                    Path.GetFileName(path),
                    ".gitkeep",
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => GetAssetKey(directory, path), StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Atelier 48V media seeding was skipped because source directory {Directory} could not be read.",
                directory);
            return [];
        }
    }

    private bool CanReadSource(string assetPath)
    {
        try
        {
            using var source = File.OpenRead(assetPath);
            if (source.Length > 0)
            {
                return true;
            }

            logger.LogWarning("Atelier 48V seed asset {AssetPath} is empty and was skipped.", assetPath);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Atelier 48V seed asset {AssetPath} is unreadable and was skipped.",
                assetPath);
        }

        return false;
    }

    private static bool IsConstructionDrawing(string assetKey) =>
        assetKey.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Contains(DrawingsDirectoryName, StringComparer.OrdinalIgnoreCase);

    private static bool HasPdfSignature(string assetPath)
    {
        Span<byte> signature = stackalloc byte[5];
        using var source = File.OpenRead(assetPath);
        return source.Read(signature) == signature.Length
            && signature.SequenceEqual("%PDF-"u8);
    }

    private static BlobUploadOptions CreateUploadOptions(
        string contentType,
        string assetPath,
        string assetKey,
        bool inline)
    {
        var fileName = Path.GetFileName(assetPath);
        return new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                ContentDisposition = $"{(inline ? "inline" : "attachment")}; filename*=UTF-8''{Uri.EscapeDataString(fileName)}",
            },
            Metadata = new Dictionary<string, string>
            {
                ["sourceFileNameUtf8"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName)),
                ["sourceRelativePathUtf8"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(assetKey)),
            },
        };
    }

    private static string GetAssetKey(string rootDirectory, string assetPath) =>
        Path.GetRelativePath(rootDirectory, assetPath)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static string NormalizeRoomKey(string? value) =>
        (SiteMediaPolicy.NormalizeCategory(value) ?? string.Empty).ToUpperInvariant();

    private static Guid CreateDeterministicGuid(params string[] values)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', values)));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static void SetAuditValues(
        Domain.SeedWork.BaseAuditableEntity entity,
        bool preserveCreated = false)
    {
        var now = DateTimeOffset.UtcNow;
        if (!preserveCreated)
        {
            entity.Created = now;
            entity.CreatedBy = SeededBy;
        }

        entity.LastModified = now;
        entity.LastModifiedBy = SeededBy;
    }

    private static async Task<int?> GetVideoDurationSecondsAsync(
        string assetPath,
        CancellationToken cancellationToken)
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

        if (process.ExitCode != 0
            || !double.TryParse(
                output.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var duration))
        {
            return null;
        }

        return Math.Max(0, (int)Math.Round(duration, MidpointRounding.AwayFromZero));
    }
}
