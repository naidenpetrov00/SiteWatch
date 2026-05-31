using Domain.SeedWork;

namespace Domain.Entities;

public sealed class InvoiceLine : BaseEntity
{
    private InvoiceLine()
    {
    }

    public Guid InvoiceDocumentId { get; private set; }
    public int LineNumber { get; private set; }
    public string? Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }

    public InvoiceDocument InvoiceDocument { get; private set; } = null!;
}
