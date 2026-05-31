namespace Application.Invoices.Queries;

public sealed record InvoiceLineDto(
    Guid Id,
    string? ProductCode,
    string? ProductName,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    decimal? Discount,
    decimal? VatRate,
    decimal? LineTotal,
    decimal? Confidence);
