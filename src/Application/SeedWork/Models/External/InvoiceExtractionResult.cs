using System.Collections.Immutable;

namespace Application.SeedWork.Models.External;

public sealed record InvoiceExtractionResult
{
    public InvoiceExtractionDocumentType DocumentType { get; init; } = InvoiceExtractionDocumentType.Unknown;

    public decimal? DocumentTypeConfidence { get; init; }

    public string? SupplierName { get; init; }

    public decimal? SupplierNameConfidence { get; init; }

    public string? SupplierEik { get; init; }

    public decimal? SupplierEikConfidence { get; init; }

    public string? SupplierVatNumber { get; init; }

    public decimal? SupplierVatNumberConfidence { get; init; }

    public string? BuyerName { get; init; }

    public decimal? BuyerNameConfidence { get; init; }

    public string? InvoiceNumber { get; init; }

    public decimal? InvoiceNumberConfidence { get; init; }

    public DateTimeOffset? InvoiceDate { get; init; }

    public decimal? InvoiceDateConfidence { get; init; }

    public string? Currency { get; init; }

    public decimal? CurrencyConfidence { get; init; }

    public decimal? NetTotal { get; init; }

    public decimal? NetTotalBgn { get; init; }

    public decimal? NetTotalBgnConfidence { get; init; }

    public decimal? NetTotalEur { get; init; }

    public decimal? NetTotalEurConfidence { get; init; }

    public decimal? NetTotalConfidence { get; init; }

    public decimal? VatTotal { get; init; }

    public decimal? VatTotalBgn { get; init; }

    public decimal? VatTotalBgnConfidence { get; init; }

    public decimal? VatTotalEur { get; init; }

    public decimal? VatTotalEurConfidence { get; init; }

    public decimal? VatTotalConfidence { get; init; }

    public decimal? GrossTotal { get; init; }

    public decimal? GrossTotalBgn { get; init; }

    public decimal? GrossTotalBgnConfidence { get; init; }

    public decimal? GrossTotalEur { get; init; }

    public decimal? GrossTotalEurConfidence { get; init; }

    public decimal? GrossTotalConfidence { get; init; }

    public decimal? OverallConfidence { get; init; }

    public ImmutableArray<InvoiceExtractionLineResult> Items { get; init; } = ImmutableArray<InvoiceExtractionLineResult>.Empty;

    public ImmutableArray<InvoiceExtractionIssueResult> Issues { get; init; } = ImmutableArray<InvoiceExtractionIssueResult>.Empty;

    public string? RawJson { get; init; }

    public string? RawOcrText { get; init; }
}
