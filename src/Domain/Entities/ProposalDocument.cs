using Domain.SeedWork;

namespace Domain.Entities;

public sealed class ProposalDocument : BaseEntity
{
    private ProposalDocument()
    {
    }

    public Guid ProposalId { get; private set; }
    public Proposal Proposal { get; private set; } = null!;
    public string BlobName { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long ContentLength { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;
    public DateTimeOffset StoredAt { get; private set; }

    public static ProposalDocument Create(
        Proposal proposal,
        string blobName,
        string fileName,
        string contentType,
        long contentLength,
        string sha256,
        DateTimeOffset storedAt)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contentLength);

        return new ProposalDocument
        {
            Id = Guid.NewGuid(),
            Proposal = proposal,
            ProposalId = proposal.Id,
            BlobName = blobName,
            FileName = fileName,
            ContentType = contentType,
            ContentLength = contentLength,
            Sha256 = sha256,
            StoredAt = storedAt
        };
    }
}
