using Domain.SeedWork.Enums;
using FluentValidation;

namespace Application.Offers.Queries;

public sealed class SiteOffersQueryValidator : AbstractValidator<SiteOffersQuery>
{
    public SiteOffersQueryValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 1000);
        RuleFor(query => query.NumberId)
            .Must(value => string.IsNullOrWhiteSpace(value) || int.TryParse(value, out _))
            .WithMessage("NumberId must be a valid integer.");
        RuleFor(query => query.Status)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || Enum.TryParse<OfferStatus>(value, true, out var status)
                && Enum.IsDefined(status))
            .WithMessage("Status must be a valid offer status.");
        RuleFor(query => query.SortActive)
            .Must(sort => string.IsNullOrWhiteSpace(sort)
                || SiteOffersQuery.Table.Sorts.ContainsKey(sort.Trim()))
            .WithMessage("SortActive must be one of the offer columns.");
        RuleFor(query => query.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction)
                || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be asc or desc.");
    }
}
