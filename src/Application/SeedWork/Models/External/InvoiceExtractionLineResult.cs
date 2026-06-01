namespace Application.SeedWork.Models.External;

public sealed record InvoiceExtractionLineResult
{
    public string? ProductCode { get; init; }

    public string? ProductName { get; init; }

    public decimal? Quantity { get; init; }

    public string? Unit { get; init; }

    public decimal? UnitPrice { get; init; }

    public decimal? Discount { get; init; }

    public decimal? VatRate { get; init; }

    public decimal? LineTotal { get; init; }

    public decimal? Confidence { get; init; }
}
