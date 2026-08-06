using System.Text;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Ardalis.GuardClauses;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class BlobInvoiceService(BlobServiceClient blobServiceClient)
    : IInvoiceBlobService
{
    private const string FileNameMetadataKey = "filename";

    public async Task UploadAsync(
        Guid invoiceId,
        UploadedInvoiceFile file,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(Guard.Against.NullOrWhiteSpace(file.FileName));
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType.Trim();
        var blobClient = GetBlobClient(invoiceId);
        var encodedFileName = Uri.EscapeDataString(fileName);

        await blobClient.UploadAsync(
            file.Stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = $"inline; filename*=UTF-8''{encodedFileName}"
                },
                Metadata = new Dictionary<string, string>
                {
                    [FileNameMetadataKey] = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName))
                }
            },
            cancellationToken);
    }

    public async Task DeleteIfExistsAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        await GetBlobClient(invoiceId).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<InvoiceFileInfoDto> GetInfoAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = GetBlobClient(invoiceId);
        BlobProperties properties;

        try
        {
            properties = (await blobClient.GetPropertiesAsync(
                cancellationToken: cancellationToken)).Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new NotFoundException("Invoice file", invoiceId.ToString());
        }

        var fileName = DecodeFileName(properties.Metadata, invoiceId);
        var contentType = string.IsNullOrWhiteSpace(properties.ContentType)
            ? "application/octet-stream"
            : properties.ContentType;

        return new InvoiceFileInfoDto(fileName, contentType);
    }

    public async Task<InvoiceFileResponse> DownloadAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = GetBlobClient(invoiceId);
        BlobDownloadStreamingResult download;

        try
        {
            download = (await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken)).Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new NotFoundException("Invoice file", invoiceId.ToString());
        }

        var fileName = DecodeFileName(download.Details.Metadata, invoiceId);
        var contentType = string.IsNullOrWhiteSpace(download.Details.ContentType)
            ? "application/octet-stream"
            : download.Details.ContentType;

        return new InvoiceFileResponse(
            download.Content,
            fileName,
            contentType,
            download.Details.ContentLength);
    }

    private BlobClient GetBlobClient(Guid invoiceId) => blobServiceClient
        .GetBlobContainerClient(BlobContainerName.Invoices.ToString())
        .GetBlobClient(invoiceId.ToString());

    private static string DecodeFileName(
        IDictionary<string, string> metadata,
        Guid invoiceId)
    {
        if (!metadata.TryGetValue(FileNameMetadataKey, out var encodedFileName))
        {
            return $"invoice-{invoiceId}.pdf";
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedFileName));
        }
        catch (FormatException)
        {
            return $"invoice-{invoiceId}.pdf";
        }
    }
}
