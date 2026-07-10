using System.Globalization;
using FluentValidation;

namespace Application.Invoices.Queries;

public sealed class DashboardInvoicesQueryValidator : AbstractValidator<DashboardInvoicesQuery>
{
    public DashboardInvoicesQueryValidator()
    {
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 1000);
        RuleFor(query => query.Id)
            .Must(id => string.IsNullOrWhiteSpace(id) || Guid.TryParse(id, out _))
            .WithMessage("Id must be a valid GUID.");
        RuleFor(query => query.SortActive)
            .Must(
                sort =>
                    string.IsNullOrWhiteSpace(sort)
                    || DashboardInvoicesQuery.Table.Sorts.ContainsKey(sort.Trim())
            )
            .WithMessage("SortActive must be one of the invoice columns.");
        RuleFor(query => query.SortDirection)
            .Must(
                direction =>
                    string.IsNullOrWhiteSpace(direction)
                    || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || direction.Equals("desc", StringComparison.OrdinalIgnoreCase)
            )
            .WithMessage("SortDirection must be asc or desc.");
        RuleFor(query => query.TotalValueExcludingVat)
            .Must(BeValidDecimal)
            .WithMessage("TotalValueExcludingVat must be a valid decimal value.");
        RuleFor(query => query.Vat)
            .Must(BeValidDecimal)
            .WithMessage("Vat must be a valid decimal value.");
        RuleFor(query => query.TotalValueIncludingVat)
            .Must(BeValidDecimal)
            .WithMessage("TotalValueIncludingVat must be a valid decimal value.");
    }

    private static bool BeValidDecimal(string? rawValue)
    {
        var normalizedValue = rawValue?.Trim() ?? string.Empty;
        return normalizedValue.Length == 0
            || decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
    }
}
