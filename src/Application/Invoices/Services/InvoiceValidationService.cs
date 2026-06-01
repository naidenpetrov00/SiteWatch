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
        }

        if (line.UnitPrice is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.UnitPrice",
                null,
                "UnitPrice is required for each line item.",
                line.Confidence));
        }

        if (line.LineTotal is null)
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.LineTotal",
                null,
                "LineTotal is required for each line item.",
                line.Confidence));
        }

        if (line.Quantity is null || line.UnitPrice is null || line.LineTotal is null) return;
        var expected = line.Quantity.Value * line.UnitPrice.Value;
        if (!ApproximatelyEqual(expected, line.LineTotal.Value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                $"{linePrefix}.LineTotal",
                line.LineTotal.Value.ToString(CultureInfo.InvariantCulture),
                "Quantity multiplied by UnitPrice must approximately equal LineTotal.",
                line.Confidence));
        }
    }

    private static void ValidateTotals(
        InvoiceExtractionResult invoiceExtractionResult,
        List<InvoiceValidationIssueResult> issues)
    {
        var items = invoiceExtractionResult.Items.IsDefault
            ? ImmutableArray<InvoiceExtractionLineResult>.Empty
            : invoiceExtractionResult.Items;

        var lineTotals = items
            .Where(x => x.LineTotal is not null)
            .Select(x => x.LineTotal!.Value)
            .Sum();

        if (invoiceExtractionResult.NetTotal is not null && !ApproximatelyEqual(lineTotals, invoiceExtractionResult.NetTotal.Value))
        {
            issues.Add(new InvoiceValidationIssueResult(
                "NetTotal",
                invoiceExtractionResult.NetTotal.Value.ToString(CultureInfo.InvariantCulture),
                "Sum of line totals must approximately equal NetTotal.",
                invoiceExtractionResult.OverallConfidence));
        }

        if (invoiceExtractionResult.NetTotal is not null &&
            invoiceExtractionResult.VatTotal is not null &&
            invoiceExtractionResult.GrossTotal is not null)
        {
            var expectedGrossTotal = invoiceExtractionResult.NetTotal.Value + invoiceExtractionResult.VatTotal.Value;
            if (!ApproximatelyEqual(expectedGrossTotal, invoiceExtractionResult.GrossTotal.Value))
            {
                issues.Add(new InvoiceValidationIssueResult(
                    "GrossTotal",
                    invoiceExtractionResult.GrossTotal.Value.ToString(CultureInfo.InvariantCulture),
                    "NetTotal plus VatTotal must approximately equal GrossTotal.",
                    invoiceExtractionResult.OverallConfidence));
            }
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
