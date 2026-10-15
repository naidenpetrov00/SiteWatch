using System.Security.Cryptography;
using System.Text;
using Application.Proposals;
using Application.SeedWork.Models.Internal;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ardalis.GuardClauses;

namespace Infrastructure.Proposals;

internal sealed record StoredProposalDocument(
    string BlobName,
    string FileName,
    string ContentType,
    long ContentLength,
    string Sha256,
    DateTimeOffset StoredAt);

internal sealed class ProposalDocumentBlobStorage(BlobServiceClient blobServiceClient)
{
    private const string PdfContentType = "application/pdf";

    public async Task<StoredProposalDocument> UploadAsync(
        Guid proposalId,
        int proposalNumber,
        int revisionNumber,
        byte[] content,
        DateTimeOffset storedAt,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfZero(content.Length);
        var fileName = $"proposal-{proposalNumber}-r{revisionNumber}.pdf";
        var blobName = $"{proposalId:N}/{Guid.NewGuid():N}.pdf";
        var encodedFileName = Uri.EscapeDataString(fileName);
        var blobClient = GetContainer().GetBlobClient(blobName);

        await using var stream = new MemoryStream(content, writable: false);
        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = PdfContentType,
                    ContentDisposition = $"attachment; filename*=UTF-8''{encodedFileName}"
                },
                Metadata = new Dictionary<string, string>
                {
                    ["filename"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName)),
                    ["proposalid"] = proposalId.ToString("N"),
                    ["revision"] = revisionNumber.ToString()
                }
            },
            cancellationToken);

        return new StoredProposalDocument(
            blobName,
            fileName,
            PdfContentType,
            content.LongLength,
            Convert.ToHexString(SHA256.HashData(content)),
            storedAt);
    }

    public async Task DeleteIfExistsAsync(
        string blobName,
        CancellationToken cancellationToken) =>
        await GetContainer()
            .GetBlobClient(blobName)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);

    public async Task<ProposalFileResponse> DownloadAsync(
        string blobName,
        string fileName,
        CancellationToken cancellationToken)
    {
        BlobDownloadStreamingResult download;
        try
        {
            download = (await GetContainer()
                .GetBlobClient(blobName)
                .DownloadStreamingAsync(cancellationToken: cancellationToken)).Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            throw new NotFoundException("Proposal PDF", blobName);
        }

        return new ProposalFileResponse(
            download.Content,
            fileName,
            string.IsNullOrWhiteSpace(download.Details.ContentType)
                ? PdfContentType
                : download.Details.ContentType,
            download.Details.ContentLength);
    }

    private BlobContainerClient GetContainer() => blobServiceClient
        .GetBlobContainerClient(BlobContainerName.Proposals.ToString());
}
