using Domain.SeedWork;

namespace Domain.Entities;

public sealed class InvoiceReviewIssue : BaseEntity
{
    private InvoiceReviewIssue()
    {
    }

    public Guid InvoiceDocumentId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public bool IsResolved { get; private set; }

    public InvoiceDocument InvoiceDocument { get; private set; } = null!;
}
