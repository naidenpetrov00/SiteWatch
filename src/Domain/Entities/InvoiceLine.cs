using Domain.SeedWork;

namespace Domain.Entities;

public sealed class InvoiceLine : BaseEntity
{
    private InvoiceLine()
    {
    }

    public Guid InvoiceDocumentId { get; private set; }
    public string? ProductCode { get; private set; }
    public string? ProductName { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? Discount { get; private set; }
    public decimal? VatRate { get; private set; }
    public decimal? LineTotal { get; private set; }
    public decimal? Confidence { get; private set; }

    public InvoiceDocument InvoiceDocument { get; private set; } = null!;

    public static InvoiceLine Create(
        Guid invoiceDocumentId,
        string? productCode,
        string? productName,
        decimal? quantity,
        string? unit,
        decimal? unitPrice,
        decimal? discount,
        decimal? vatRate,
        decimal? lineTotal,
        decimal? confidence)
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceDocumentId = invoiceDocumentId,
            ProductCode = productCode,
            ProductName = productName,
            Quantity = quantity,
            Unit = unit,
            UnitPrice = unitPrice,
            Discount = discount,
            VatRate = vatRate,
            LineTotal = lineTotal,
            Confidence = confidence
        };
}
