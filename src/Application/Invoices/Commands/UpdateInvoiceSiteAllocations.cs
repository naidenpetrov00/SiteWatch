using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Invoices.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateInvoiceSiteAllocationsCommand : IRequest
{
    public Guid InvoiceId { get; set; }
    public List<InvoiceSiteAllocationInput> SiteAllocations { get; init; } = [];
}

public sealed class UpdateInvoiceSiteAllocationsValidator
    : AbstractValidator<UpdateInvoiceSiteAllocationsCommand>
{
    public UpdateInvoiceSiteAllocationsValidator()
    {
        RuleFor(command => command.InvoiceId).NotEmpty();
        RuleFor(command => command.SiteAllocations).NotNull();
        RuleForEach(command => command.SiteAllocations)
            .SetValidator(new InvoiceSiteAllocationInputValidator());
        RuleFor(command => command.SiteAllocations)
            .Must(InvoiceSiteAllocationValidation.HaveUniqueSites)
            .WithMessage("A site can only be allocated once per invoice.");
    }
}

public sealed class UpdateInvoiceSiteAllocationsHandler(IInvoiceService invoiceService)
    : IRequestHandler<UpdateInvoiceSiteAllocationsCommand>
{
    public Task Handle(
        UpdateInvoiceSiteAllocationsCommand request,
        CancellationToken cancellationToken) =>
        invoiceService.UpdateSiteAllocationsAsync(request, cancellationToken);
}
