using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Storage;
using NSubstitute;

namespace Infrastructure.Tests.Storage;

public sealed class BlobMediaServicesTests
{
    [Fact]
    public async Task UploadImageAsync_does_not_access_blob_storage_when_thumbnail_validation_fails()
    {
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var imagesService = Substitute.For<IImagesService>();
        var validationFailure = InvalidFile();
        imagesService.CreateThumbnailAsync(
                Arg.Any<Stream>(),
                "image/jpeg",
                CancellationToken.None)
            .Returns(Task.FromException<Stream>(validationFailure));
        var service = new BlobImagesService(blobServiceClient, imagesService);
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.UploadImageAsync(
            content,
            "image/jpeg",
            BlobContainerName.Images,
            CancellationToken.None));

        Assert.Same(validationFailure, exception);
        blobServiceClient.DidNotReceiveWithAnyArgs().GetBlobContainerClient(default!);
    }

    [Fact]
    public async Task UploadImageAsync_processes_before_upload_and_deletes_original_when_thumbnail_upload_fails()
    {
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var container = Substitute.For<BlobContainerClient>();
        var originalBlob = Substitute.For<BlobClient>();
        var thumbnailBlob = Substitute.For<BlobClient>();
        var imagesService = Substitute.For<IImagesService>();
        blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString()).Returns(container);
        container.GetBlobClient(Arg.Any<string>()).Returns(originalBlob, thumbnailBlob);
        imagesService.CreateThumbnailAsync(
                Arg.Any<Stream>(),
                "image/jpeg",
                CancellationToken.None)
            .Returns(_ => Task.FromResult<Stream>(new MemoryStream([0xFF, 0xD8, 0xFF])));
        CompleteUpload(originalBlob);
        var uploadFailure = new IOException("thumbnail upload failed");
        FailUpload(thumbnailBlob, uploadFailure);
        CompleteDelete(originalBlob);
        var service = new BlobImagesService(blobServiceClient, imagesService);
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<IOException>(() => service.UploadImageAsync(
            content,
            "image/jpeg",
            BlobContainerName.Images,
            CancellationToken.None));

        Assert.Same(uploadFailure, exception);
        Received.InOrder(() =>
        {
            imagesService.CreateThumbnailAsync(
                Arg.Any<Stream>(),
                "image/jpeg",
                CancellationToken.None);
            originalBlob.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: CancellationToken.None);
            thumbnailBlob.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: CancellationToken.None);
        });
        await originalBlob.Received(1).DeleteIfExistsAsync(
            cancellationToken: CancellationToken.None);
    }

    [Fact]
    public async Task UploadVideoAsync_does_not_access_blob_storage_when_inspection_fails()
    {
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var videosService = Substitute.For<IVideosService>();
        var inspector = Substitute.For<IVideoFileInspector>();
        var validationFailure = InvalidFile();
        inspector.InspectAsync(Arg.Any<string>(), "video/mp4", CancellationToken.None)
            .Returns(Task.FromException<VideoInspectionResult>(validationFailure));
        var service = new BlobVideosService(blobServiceClient, videosService, inspector);
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.UploadVideoAsync(
            content,
            "video/mp4",
            BlobContainerName.Videos,
            CancellationToken.None));

        Assert.Same(validationFailure, exception);
        await videosService.DidNotReceiveWithAnyArgs().CreateSnapshotAsync(default!, default);
        blobServiceClient.DidNotReceiveWithAnyArgs().GetBlobContainerClient(default!);
    }

    [Fact]
    public async Task UploadVideoAsync_does_not_access_blob_storage_when_snapshot_generation_fails()
    {
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var videosService = Substitute.For<IVideosService>();
        var inspector = Substitute.For<IVideoFileInspector>();
        inspector.InspectAsync(Arg.Any<string>(), "video/webm", CancellationToken.None)
            .Returns(new VideoInspectionResult(4));
        var validationFailure = InvalidFile();
        videosService.CreateSnapshotAsync(Arg.Any<Stream>(), CancellationToken.None)
            .Returns(Task.FromException<Stream>(validationFailure));
        var service = new BlobVideosService(blobServiceClient, videosService, inspector);
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.UploadVideoAsync(
            content,
            "video/webm",
            BlobContainerName.Videos,
            CancellationToken.None));

        Assert.Same(validationFailure, exception);
        blobServiceClient.DidNotReceiveWithAnyArgs().GetBlobContainerClient(default!);
    }

    [Fact]
    public async Task UploadVideoAsync_processes_before_upload_and_deletes_video_when_snapshot_upload_fails()
    {
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var videoContainer = Substitute.For<BlobContainerClient>();
        var imageContainer = Substitute.For<BlobContainerClient>();
        var videoBlob = Substitute.For<BlobClient>();
        var snapshotBlob = Substitute.For<BlobClient>();
        var videosService = Substitute.For<IVideosService>();
        var inspector = Substitute.For<IVideoFileInspector>();
        blobServiceClient.GetBlobContainerClient(BlobContainerName.Videos.ToString()).Returns(videoContainer);
        blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString()).Returns(imageContainer);
        videoContainer.GetBlobClient(Arg.Any<string>()).Returns(videoBlob);
        imageContainer.GetBlobClient(Arg.Any<string>()).Returns(snapshotBlob);
        inspector.InspectAsync(Arg.Any<string>(), "video/mp4", CancellationToken.None)
            .Returns(new VideoInspectionResult(9));
        videosService.CreateSnapshotAsync(Arg.Any<Stream>(), CancellationToken.None)
            .Returns(_ => Task.FromResult<Stream>(new MemoryStream([0xFF, 0xD8, 0xFF])));
        CompleteUpload(videoBlob);
        var uploadFailure = new IOException("snapshot upload failed");
        FailUpload(snapshotBlob, uploadFailure);
        CompleteDelete(videoBlob);
        var service = new BlobVideosService(blobServiceClient, videosService, inspector);
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<IOException>(() => service.UploadVideoAsync(
            content,
            "video/mp4",
            BlobContainerName.Videos,
            CancellationToken.None));

        Assert.Same(uploadFailure, exception);
        Received.InOrder(() =>
        {
            inspector.InspectAsync(Arg.Any<string>(), "video/mp4", CancellationToken.None);
            videosService.CreateSnapshotAsync(Arg.Any<Stream>(), CancellationToken.None);
            videoBlob.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: CancellationToken.None);
            snapshotBlob.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: CancellationToken.None);
        });
        await videoBlob.Received(1).DeleteIfExistsAsync(
            cancellationToken: CancellationToken.None);
    }

    private static void CompleteUpload(BlobClient blobClient) =>
        blobClient.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<BlobContentInfo>>(null!));

    private static void FailUpload(BlobClient blobClient, Exception exception) =>
        blobClient.UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<BlobContentInfo>>(exception));

    private static void CompleteDelete(BlobClient blobClient) =>
        blobClient.DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<bool>>(null!));

    private static ValidationException InvalidFile() =>
        new([new ValidationFailure("file", "Invalid media.")]);
}
