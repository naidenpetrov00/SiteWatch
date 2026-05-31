using FluentValidation;

namespace Application.Invoices.Queries;

public class GetInvoicesQueryValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesQueryValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site Id is required.");
    }
}
