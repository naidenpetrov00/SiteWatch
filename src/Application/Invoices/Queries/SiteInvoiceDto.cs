namespace Application.Invoices.Queries;

/// <summary>Represents an invoice allocated to a site.</summary>
public sealed record SiteInvoiceDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public bool IsComplete { get; init; }
    public Guid? SupplierId { get; init; }
    public string? SupplierDisplayLabel { get; init; }
    public string? SubmittedFromSiteName { get; init; }
    public string? InvoiceNumber { get; init; }
    public DateTimeOffset? Date { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? TaxIdentifier { get; init; }
    public string? Address { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ContactPerson { get; init; }
    public DateTimeOffset? PaymentTerm { get; init; }
    public decimal? TotalValueExcludingVat { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? Vat { get; init; }
    public decimal? TotalValueIncludingVat { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset? PaymentTime { get; init; }
    public string? PaymentMethod { get; init; }
    public SiteInvoiceAllocationDto? SiteAllocation { get; init; }
}

/// <summary>Represents the part of an invoice allocated to the requested site.</summary>
public sealed record SiteInvoiceAllocationDto(decimal Amount, string Direction);
