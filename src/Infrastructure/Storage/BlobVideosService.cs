using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Videos.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;
using System.Globalization;

namespace Infrastructure.Storage;

internal sealed class BlobVideosService(BlobServiceClient blobServiceClient, IVideosService videosService)
    : IVideosBlobService
{
    public async Task<UploadedVideoResult> UploadVideoAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var inputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            await using (var inputFile = File.Create(inputPath))
            {
                await stream.CopyToAsync(inputFile, cancellationToken);
            }

            var durationSeconds = await GetVideoDurationSecondsAsync(inputPath, cancellationToken);

            var videoFileId = Guid.NewGuid();
            var videoBlobClient = containerClient.GetBlobClient(videoFileId.ToString());

            await using (var uploadStream = File.OpenRead(inputPath))
            {
                await videoBlobClient.UploadAsync(
                    uploadStream,
                    new BlobHttpHeaders { ContentType = contentType },
                    cancellationToken: cancellationToken);
            }

            await using var snapshotSource = File.OpenRead(inputPath);
            await using var snapshotStream = await videosService.CreateSnapshotAsync(snapshotSource, cancellationToken);
            var snapshotFileId = Guid.NewGuid();
            var snapshotContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString());
            var snapshotBlobClient = snapshotContainerClient.GetBlobClient(snapshotFileId.ToString());

            await snapshotBlobClient.UploadAsync(
                snapshotStream,
                new BlobHttpHeaders { ContentType = "image/jpeg" },
                cancellationToken: cancellationToken);

            return new UploadedVideoResult(videoFileId, snapshotFileId, durationSeconds);
        }
        finally
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }
        }
    }

    public async Task<FileResponse> DownloadVideoAsync(
        Guid fileId,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteVideoAsync(
        Guid fileId,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        var snapshotId = await videosService.DeleteVideoIdFromSiteAsync(fileId, cancellationToken);

        if (snapshotId is not null)
        {
            var snapshotContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString());
            var snapshotBlobClient = snapshotContainerClient.GetBlobClient(snapshotId.Value.ToString());
            await snapshotBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private static async Task<int?> GetVideoDurationSecondsAsync(
        string inputPath,
        CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        processStartInfo.ArgumentList.Add("-v");
        processStartInfo.ArgumentList.Add("error");
        processStartInfo.ArgumentList.Add("-show_entries");
        processStartInfo.ArgumentList.Add("format=duration");
        processStartInfo.ArgumentList.Add("-of");
        processStartInfo.ArgumentList.Add("default=noprint_wrappers=1:nokey=1");
        processStartInfo.ArgumentList.Add(inputPath);

        using var process = Process.Start(processStartInfo)
            ?? throw new InvalidOperationException("Failed to start ffprobe.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        _ = await stderrTask;

        if (process.ExitCode != 0)
        {
            return null;
        }

        if (!double.TryParse(
                stdout.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var durationSeconds))
        {
            return null;
        }

        return Math.Max(0, (int)Math.Round(durationSeconds, MidpointRounding.AwayFromZero));
    }
}
