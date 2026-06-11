using FluentValidation;

namespace Application.Identity.Queries.DashboardUsers;

public class DashboardUsersQueryValidator : AbstractValidator<DashboardUsersQuery>
{
    public DashboardUsersQueryValidator()
    {
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 500);
        RuleFor(query => query.SortActive)
            .Must(
                sort =>
                    string.IsNullOrWhiteSpace(sort)
                    || DashboardUsersQuery.Table.Sorts.ContainsKey(sort.Trim())
            )
            .WithMessage("SortActive must be one of the dashboard user columns.");
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
