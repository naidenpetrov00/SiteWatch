using FluentValidation;

namespace Application.Invoices.Queries;

public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site Id is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("Invoice Id is required.");
    }
}
