using System.Collections.Immutable;

namespace Application.SeedWork.Models.External;

public sealed record InvoiceExtractionResult
{
    public string? DocumentType { get; init; }

    public string? SupplierName { get; init; }

    public string? SupplierEik { get; init; }

    public string? SupplierVatNumber { get; init; }

    public string? BuyerName { get; init; }

    public string? InvoiceNumber { get; init; }

    public DateTimeOffset? InvoiceDate { get; init; }

    public string? Currency { get; init; }

    public decimal? NetTotal { get; init; }

    public decimal? VatTotal { get; init; }

    public decimal? GrossTotal { get; init; }

    public decimal? OverallConfidence { get; init; }

    public ImmutableArray<InvoiceExtractionLineResult> Items { get; init; } = ImmutableArray<InvoiceExtractionLineResult>.Empty;

    public ImmutableArray<InvoiceExtractionIssueResult> Issues { get; init; } = ImmutableArray<InvoiceExtractionIssueResult>.Empty;

    public string? RawJson { get; init; }
}
