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
    public decimal? UnitPriceBgn { get; private set; }
    public decimal? UnitPriceEur { get; private set; }
    public decimal? Discount { get; private set; }
    public decimal? DiscountBgn { get; private set; }
    public decimal? DiscountEur { get; private set; }
    public decimal? VatRate { get; private set; }
    public decimal? LineTotal { get; private set; }
    public decimal? LineTotalBgn { get; private set; }
    public decimal? LineTotalEur { get; private set; }
    public decimal? Confidence { get; private set; }

    public InvoiceDocument InvoiceDocument { get; private set; } = null!;

    public static InvoiceLine Create(
        Guid invoiceDocumentId,
        string? productCode,
        string? productName,
        decimal? quantity,
        string? unit,
        decimal? unitPrice,
        decimal? unitPriceBgn,
        decimal? unitPriceEur,
        decimal? discount,
        decimal? discountBgn,
        decimal? discountEur,
        decimal? vatRate,
        decimal? lineTotal,
        decimal? lineTotalBgn,
        decimal? lineTotalEur,
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
            UnitPriceBgn = unitPriceBgn,
            UnitPriceEur = unitPriceEur,
            Discount = discount,
            DiscountBgn = discountBgn,
            DiscountEur = discountEur,
            VatRate = vatRate,
            LineTotal = lineTotal,
            LineTotalBgn = lineTotalBgn,
            LineTotalEur = lineTotalEur,
            Confidence = confidence
        };
}
