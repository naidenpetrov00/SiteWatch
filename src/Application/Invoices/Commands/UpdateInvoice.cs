using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Invoices.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateInvoiceCommand : InvoiceDetailsCommand, IRequest
{
    public Guid InvoiceId { get; set; }
}

public sealed class UpdateInvoiceValidator : InvoiceDetailsCommandValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(command => command.InvoiceId).NotEmpty();
    }
}

public sealed class UpdateInvoiceHandler(IInvoiceService invoiceService)
    : IRequestHandler<UpdateInvoiceCommand>
{
    public Task Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken) =>
        invoiceService.UpdateAsync(request, cancellationToken);
}
