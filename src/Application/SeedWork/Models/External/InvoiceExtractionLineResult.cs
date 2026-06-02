namespace Application.SeedWork.Models.External;

public sealed record InvoiceExtractionLineResult
{
    public string? ProductCode { get; init; }

    public string? ProductName { get; init; }

    public decimal? Quantity { get; init; }

    public string? Unit { get; init; }

    public decimal? UnitPrice { get; init; }

    public decimal? UnitPriceBgn { get; init; }

    public decimal? UnitPriceBgnConfidence { get; init; }

    public decimal? UnitPriceEur { get; init; }

    public decimal? UnitPriceEurConfidence { get; init; }

    public decimal? Discount { get; init; }

    public decimal? DiscountBgn { get; init; }

    public decimal? DiscountBgnConfidence { get; init; }

    public decimal? DiscountEur { get; init; }

    public decimal? DiscountEurConfidence { get; init; }

    public decimal? VatRate { get; init; }

    public decimal? LineTotal { get; init; }

    public decimal? LineTotalBgn { get; init; }

    public decimal? LineTotalBgnConfidence { get; init; }

    public decimal? LineTotalEur { get; init; }

    public decimal? LineTotalEurConfidence { get; init; }

    public decimal? Confidence { get; init; }
}
