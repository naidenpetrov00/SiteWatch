using System.Text;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Ardalis.GuardClauses;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

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

    public async Task<InvoiceFileAccessDto> CreateReadAccessAsync(
        Guid invoiceId,
        TimeSpan lifetime,
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

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "The configured blob credentials cannot generate invoice file access links.");
        }

        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        var fileName = DecodeFileName(properties.Metadata, invoiceId);
        var contentType = string.IsNullOrWhiteSpace(properties.ContentType)
            ? "application/octet-stream"
            : properties.ContentType;
        var contentDisposition = string.IsNullOrWhiteSpace(properties.ContentDisposition)
            ? $"inline; filename*=UTF-8''{Uri.EscapeDataString(fileName)}"
            : properties.ContentDisposition;
        var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresAt)
        {
            BlobContainerName = BlobContainerName.Invoices.ToString(),
            BlobName = invoiceId.ToString(),
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ContentDisposition = contentDisposition,
            ContentType = contentType
        };

        return new InvoiceFileAccessDto(
            blobClient.GenerateSasUri(sasBuilder).ToString(),
            fileName,
            contentType,
            expiresAt);
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
