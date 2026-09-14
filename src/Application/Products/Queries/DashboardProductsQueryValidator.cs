using System.Globalization;
using FluentValidation;

namespace Application.Products.Queries;

public sealed class DashboardProductsQueryValidator : AbstractValidator<DashboardProductsQuery>
{
    public DashboardProductsQueryValidator()
    {
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 1000);
        RuleFor(query => query.NumberId)
            .Must(value => string.IsNullOrWhiteSpace(value) || int.TryParse(value, out _))
            .WithMessage("NumberId must be a valid integer.");
        RuleFor(query => query.Id)
            .Must(value => string.IsNullOrWhiteSpace(value) || Guid.TryParse(value, out _))
            .WithMessage("Id must be a valid GUID.");
        RuleFor(query => query.PackageQuantity)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out _))
            .WithMessage("PackageQuantity must be a valid decimal number.");
        RuleFor(query => query.Status)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || Enum.TryParse<Domain.SeedWork.Enums.ProductStatus>(value, true, out var status)
                    && Enum.IsDefined(status))
            .WithMessage("Status must be Active, Unavailable, or Discontinued.");
        RuleFor(query => query.SortActive)
            .Must(sort => string.IsNullOrWhiteSpace(sort)
                || DashboardProductsQuery.Table.Sorts.ContainsKey(sort.Trim()))
            .WithMessage("SortActive must be one of the product columns.");
        RuleFor(query => query.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction)
                || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be asc or desc.");
    }
}
