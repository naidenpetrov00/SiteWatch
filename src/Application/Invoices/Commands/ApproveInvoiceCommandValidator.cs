using FluentValidation;

namespace Application.Invoices.Commands;

public class ApproveInvoiceCommandValidator : AbstractValidator<ApproveInvoiceCommand>
{
    public ApproveInvoiceCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site Id is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("Invoice Id is required.");
    }
}
