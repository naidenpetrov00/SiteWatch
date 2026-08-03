namespace Application.Invoices.Queries;

/// <summary>Represents an invoice allocated to a site.</summary>
public sealed record SiteInvoiceDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public Guid SupplierId { get; init; }
    public string SupplierDisplayLabel { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTimeOffset Date { get; init; }
    public string TaxIdentifier { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
    public DateTimeOffset PaymentTerm { get; init; }
    public decimal TotalValueExcludingVat { get; init; }
    public decimal Vat { get; init; }
    public decimal TotalValueIncludingVat { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset? PaymentTime { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public required SiteInvoiceAllocationDto SiteAllocation { get; init; }
}

/// <summary>Represents the part of an invoice allocated to the requested site.</summary>
public sealed record SiteInvoiceAllocationDto(decimal Amount, string Direction);
