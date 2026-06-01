using FluentValidation;

namespace Application.Invoices.Commands;

public sealed class ProcessInvoiceCommandValidator : AbstractValidator<ProcessInvoiceCommand>
{
    public ProcessInvoiceCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site Id is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("Invoice Id is required.");
    }
}
