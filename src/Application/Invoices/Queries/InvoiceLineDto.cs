namespace Application.Invoices.Queries;

public sealed record InvoiceLineDto(
    Guid Id,
    string? ProductCode,
    string? ProductName,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    decimal? UnitPriceBgn,
    decimal? UnitPriceEur,
    decimal? Discount,
    decimal? DiscountBgn,
    decimal? DiscountEur,
    decimal? VatRate,
    decimal? LineTotal,
    decimal? LineTotalBgn,
    decimal? LineTotalEur,
    decimal? Confidence);
