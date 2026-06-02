using System.Collections.Immutable;
using System.Globalization;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;

namespace Application.Invoices.Services;

public sealed class InvoiceValidationService : IInvoiceValidationService
{
    private const decimal Tolerance = 0.02m;
    private const decimal MinimumConfidence = 0.80m;
    private static readonly HashSet<string> AllowedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BGN",
        "EUR",
        "USD"
    };

    public Task<InvoiceValidationResult> ValidateAsync(
        InvoiceExtractionResult invoiceExtractionResult,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        var issues = new List<InvoiceValidationIssueResult>();

        ValidateRequiredDocumentType(invoiceExtractionResult.DocumentType, "DocumentType", issues);
        ValidateRequiredString(invoiceExtractionResult.SupplierName, "SupplierName", issues);
        ValidateRequiredString(invoiceExtractionResult.InvoiceNumber, "InvoiceNumber", issues);
        ValidateRequiredDate(invoiceExtractionResult.InvoiceDate, "InvoiceDate", issues);
        ValidateRequiredDecimal(invoiceExtractionResult.NetTotal, "NetTotal", issues);
        ValidateRequiredDecimal(invoiceExtractionResult.VatTotal, "VatTotal", issues);
        ValidateRequiredDecimal(invoiceExtractionResult.GrossTotal, "GrossTotal", issues);

        if (!string.IsNullOrWhiteSpace(invoiceExtractionResult.Currency) &&
            !AllowedCurrencies.Contains(invoiceExtractionResult.Currency))
        {
            issues.Add(new InvoiceValidationIssueResult(
                "Currency",
                invoiceExtractionResult.Currency,
                "Currency must be BGN, EUR, USD, or null.",
                null));
        }

        var lines = invoiceExtractionResult.Items.IsDefault
            ? ImmutableArray<InvoiceExtractionLineResult>.Empty
            : invoiceExtractionResult.Items;
        if (!lines.Any())
        {
            issues.Add(new InvoiceValidationIssueResult(
                "Items",
                null,
                "Document must contain at least one line item.",
                null));
        }

        foreach (var (line, index) in lines.Select((value, valueIndex) => (value, valueIndex + 1)))
        {
            ValidateLine(line, index, issues);
        }

        if (invoiceExtractionResult.OverallConfidence is not null &&
            invoiceExtractionResult.OverallConfidence < MinimumConfidence)
        {
            issues.Add(new InvoiceValidationIssueResult(
                "OverallConfidence",
                invoiceExtractionResult.OverallConfidence.Value.ToString(CultureInfo.InvariantCulture),
                $"Overall confidence must be at least {MinimumConfidence.ToString(CultureInfo.InvariantCulture)}.",
                invoiceExtractionResult.OverallConfidence));
        }

        ValidateTotals(invoiceExtractionResult, issues);

        return Task.FromResult(new InvoiceValidationResult(issues.Count == 0, issues));
    }

    private static void ValidateLine(
        InvoiceExtractionLineResult line,
        int lineIndex,
        List<InvoiceValidationIssueResult> issues)
    {
        var linePrefix = $"Items[{lineIndex}]";
        if (line.Quantity is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.Quantity",
                null,
                "Quantity is required for each line item.",
                line.Confidence));
            return;
        }

        ValidateLineAmount(
            line,
            lineIndex,
            suffix: string.Empty,
            line.UnitPrice,
            line.LineTotal,
            line.Confidence,
            issues);

        ValidateLineAmount(
            line,
            lineIndex,
            suffix: "Bgn",
            line.UnitPriceBgn,
            line.LineTotalBgn,
            line.UnitPriceBgnConfidence ?? line.LineTotalBgnConfidence ?? line.Confidence,
            issues);

        ValidateLineAmount(
            line,
            lineIndex,
            suffix: "Eur",
            line.UnitPriceEur,
            line.LineTotalEur,
            line.UnitPriceEurConfidence ?? line.LineTotalEurConfidence ?? line.Confidence,
            issues);
    }

    private static void ValidateTotals(
        InvoiceExtractionResult invoiceExtractionResult,
        List<InvoiceValidationIssueResult> issues)
    {
        ValidateDocumentTotals(
            invoiceExtractionResult,
            suffix: string.Empty,
            invoiceExtractionResult.NetTotal,
            invoiceExtractionResult.VatTotal,
            invoiceExtractionResult.GrossTotal,
            invoiceExtractionResult.OverallConfidence,
            issues);

        ValidateDocumentTotals(
            invoiceExtractionResult,
            suffix: "Bgn",
            invoiceExtractionResult.NetTotalBgn,
            invoiceExtractionResult.VatTotalBgn,
            invoiceExtractionResult.GrossTotalBgn,
            invoiceExtractionResult.NetTotalBgnConfidence
            ?? invoiceExtractionResult.VatTotalBgnConfidence
            ?? invoiceExtractionResult.GrossTotalBgnConfidence
            ?? invoiceExtractionResult.OverallConfidence,
            issues);

        ValidateDocumentTotals(
            invoiceExtractionResult,
            suffix: "Eur",
            invoiceExtractionResult.NetTotalEur,
            invoiceExtractionResult.VatTotalEur,
            invoiceExtractionResult.GrossTotalEur,
            invoiceExtractionResult.NetTotalEurConfidence
            ?? invoiceExtractionResult.VatTotalEurConfidence
            ?? invoiceExtractionResult.GrossTotalEurConfidence
            ?? invoiceExtractionResult.OverallConfidence,
            issues);
    }

    private static void ValidateLineAmount(
        InvoiceExtractionLineResult line,
        int lineIndex,
        string suffix,
        decimal? unitPrice,
        decimal? lineTotal,
        decimal? confidence,
        List<InvoiceValidationIssueResult> issues)
    {
        var linePrefix = $"Items[{lineIndex}]";
        var fieldUnitPrice = suffix.Length == 0 ? "UnitPrice" : $"UnitPrice{suffix}";
        var fieldLineTotal = suffix.Length == 0 ? "LineTotal" : $"LineTotal{suffix}";

        if (unitPrice is null && lineTotal is null)
        {
            return;
        }

        if (unitPrice is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.{fieldUnitPrice}",
                null,
                $"{fieldUnitPrice} is required for each line item.",
                confidence));
        }

        if (lineTotal is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.{fieldLineTotal}",
                null,
                $"{fieldLineTotal} is required for each line item.",
                confidence));
        }

        if (unitPrice is null || lineTotal is null)
        {
            return;
        }

        var expected = line.Quantity.Value * unitPrice.Value;
        if (!ApproximatelyEqual(expected, lineTotal.Value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.{fieldLineTotal}",
                lineTotal.Value.ToString(CultureInfo.InvariantCulture),
                $"Quantity multiplied by {fieldUnitPrice} must approximately equal {fieldLineTotal}.",
                confidence));
        }
    }

    private static void ValidateDocumentTotals(
        InvoiceExtractionResult invoiceExtractionResult,
        string suffix,
        decimal? netTotal,
        decimal? vatTotal,
        decimal? grossTotal,
        decimal? confidence,
        List<InvoiceValidationIssueResult> issues)
    {
        if (netTotal is null)
        {
            return;
        }

        var items = invoiceExtractionResult.Items.IsDefault
            ? ImmutableArray<InvoiceExtractionLineResult>.Empty
            : invoiceExtractionResult.Items;

        var lineTotals = items
            .Select(line => suffix.Length == 0
                ? line.LineTotal
                : suffix == "Bgn"
                    ? line.LineTotalBgn
                    : line.LineTotalEur)
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .Sum();

        if (!ApproximatelyEqual(lineTotals, netTotal.Value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                suffix.Length == 0 ? "NetTotal" : $"NetTotal{suffix}",
                netTotal.Value.ToString(CultureInfo.InvariantCulture),
                $"Sum of line totals must approximately equal {(suffix.Length == 0 ? "NetTotal" : $"NetTotal{suffix}")}.",
                confidence));
        }

        if (vatTotal is null || grossTotal is null)
        {
            return;
        }

        var expectedGrossTotal = netTotal.Value + vatTotal.Value;
        if (!ApproximatelyEqual(expectedGrossTotal, grossTotal.Value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                suffix.Length == 0 ? "GrossTotal" : $"GrossTotal{suffix}",
                grossTotal.Value.ToString(CultureInfo.InvariantCulture),
                $"{(suffix.Length == 0 ? "NetTotal" : $"NetTotal{suffix}")} plus {(suffix.Length == 0 ? "VatTotal" : $"VatTotal{suffix}")} must approximately equal {(suffix.Length == 0 ? "GrossTotal" : $"GrossTotal{suffix}")}.",
                confidence));
        }
    }

    private static void ValidateRequiredString(
        string? value,
        string fieldPath,
        List<InvoiceValidationIssueResult> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                fieldPath,
                value,
                $"{fieldPath} is required.",
                null));
        }
    }

    private static void ValidateRequiredDocumentType(
        InvoiceExtractionDocumentType value,
        string fieldPath,
        List<InvoiceValidationIssueResult> issues)
    {
        if (value == InvoiceExtractionDocumentType.Unknown)
        {
            issues.Add(new InvoiceValidationIssueResult(
                fieldPath,
                value.ToString(),
                $"{fieldPath} is required and must be Invoice, Receipt, or Offer.",
                null));
        }
    }

    private static void ValidateRequiredDate(
        DateTimeOffset? value,
        string fieldPath,
        List<InvoiceValidationIssueResult> issues)
    {
        if (value is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                fieldPath,
                null,
                $"{fieldPath} is required.",
                null));
        }
    }

    private static void ValidateRequiredDecimal(
        decimal? value,
        string fieldPath,
        List<InvoiceValidationIssueResult> issues)
    {
        if (value is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                fieldPath,
                null,
                $"{fieldPath} is required.",
                null));
        }
    }

    private static bool ApproximatelyEqual(decimal left, decimal right)
        => Math.Abs(left - right) <= Tolerance;
}
