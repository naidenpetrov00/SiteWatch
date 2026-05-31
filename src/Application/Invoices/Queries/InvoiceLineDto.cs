namespace Application.Invoices.Queries;

public sealed record InvoiceLineDto(
    Guid Id,
    int LineNumber,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);
