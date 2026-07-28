using FluentValidation;

namespace Application.Sites.Queries;

public sealed class DashboardSitesQueryValidator : AbstractValidator<DashboardSitesQuery>
{
    public DashboardSitesQueryValidator()
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
                    || DashboardSitesQuery.Table.Sorts.ContainsKey(sort.Trim())
            )
            .WithMessage("SortActive must be one of the site columns.");
        RuleFor(query => query.SortDirection)
            .Must(
                direction =>
                    string.IsNullOrWhiteSpace(direction)
                    || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || direction.Equals("desc", StringComparison.OrdinalIgnoreCase)
            )
            .WithMessage("SortDirection must be asc or desc.");
    }
}
